using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ReplacementLookup = System.Collections.Generic.Dictionary<string, (string Value, bool Preserve)>.AlternateLookup<System.ReadOnlySpan<char>>;

namespace Heroes.LocaleText;

/// <summary>
/// Used for parsing through a gamestring that has been already been xml parsed out and computed for values.
/// </summary>
[SkipLocalsInit]
internal sealed class GameStringParser
{
    private const string _hltNameAttribute = "hlt-name=";

    private static readonly SearchValues<char> _specialChars = SearchValues.Create(['<', '~', '#', '%']);
    private static readonly NumberFormatInfo _commaDecimalNumberFormat = NumberFormatInfo.ReadOnly(new NumberFormatInfo() { NumberDecimalSeparator = "," });

    private readonly string _description;
    private readonly StormLocale _gameStringLocale;
    private readonly char _decimalSeparator;

    private readonly HashSet<string>? _styleTagVariables;
    private readonly HashSet<string>? _styleConstantTagVariables;

    // created on the first parse, and only when the text has something to parse
    private List<TextRange> _textStack = null!;

    private ReplacementLookup? _valueByStyleVar;
    private ReplacementLookup? _valueByStyleConstantVar;

    private bool _isConstructed;
    private bool _isPlainText;
    private int _startingIndex;
    private int _index;

    private GameStringParser(string description, StormLocale gameStringLocale, bool extractFontVars)
    {
        _description = description;
        _gameStringLocale = gameStringLocale;
        _decimalSeparator = GetDecimalSeparator(gameStringLocale);

        ExtractFontVars = extractFontVars;
        if (ExtractFontVars)
        {
            _styleTagVariables = new(StringComparer.Ordinal);
            _styleConstantTagVariables = new(StringComparer.Ordinal);
        }
    }

    public HashSet<string>? StyleTagVariables => _styleTagVariables;

    public HashSet<string>? StyleConstantTagVariables => _styleConstantTagVariables;

    [MemberNotNullWhen(true, nameof(_styleTagVariables), nameof(StyleTagVariables), nameof(_styleConstantTagVariables), nameof(StyleConstantTagVariables))]
    public bool ExtractFontVars { get; }

    public bool RemoveHltNameFromStyleTags { get; set; }

    public bool RemoveHltNameFromConstantTags { get; set; }

    public bool UndoHltNameReplacementForStyleTags { get; set; }

    public bool UndoHltNameReplacementForConstantTags { get; set; }

    public static GameStringParser GetInstance(string gameString, StormLocale gameStringLocale = StormLocale.ENUS, bool extractFontVars = false)
    {
        return new GameStringParser(gameString, gameStringLocale, extractFontVars);
    }

    // parses (which also extracts the font vars) without building an output string
    public void EnsureParsed()
    {
        if (_isConstructed)
            return;

        _isConstructed = true;

        ReadOnlySpan<char> gameString = _description;

        // no special characters, so there's nothing to parse and every output is the original string
        if (!gameString.ContainsAny(_specialChars))
        {
            _isPlainText = true;
            return;
        }

        _textStack = new List<TextRange>(gameString.Length / 10);
        ConstructTextStack(gameString);

        // no tags or special text, so every output is the original string
        _isPlainText = _textStack.Count == 1 &&
            _textStack[0].Type == TextType.Text &&
            _textStack[0].Start == 0 &&
            _textStack[0].Length == gameString.Length;
    }

    public string GetRawText()
    {
        return BuildDescription(new GameStringFlags()
        {
            ColorTags = TagFlag.Include,
            ScalingTag = TagFlag.Include,
            NewLineTag = TagFlag.Include,
            ErrorTag = TagFlag.Include,
            SpaceTag = TagFlag.Include,
        });
    }

    public string GetPlainText(bool includeNewLineTags, bool includeScaling)
    {
        return BuildDescription(new GameStringFlags()
        {
            ColorTags = TagFlag.None,
            ScalingTag = includeScaling ? TagFlag.Eval : TagFlag.None,
            NewLineTag = includeNewLineTags ? TagFlag.Include : TagFlag.Eval,
            ErrorTag = TagFlag.None,
            SpaceTag = TagFlag.Eval,
        });
    }

    public string GetColoredText(bool includeScaling)
    {
        return BuildDescription(new GameStringFlags()
        {
            ColorTags = TagFlag.Include,
            ScalingTag = includeScaling ? TagFlag.Eval : TagFlag.None,
            NewLineTag = TagFlag.Include,
            ErrorTag = TagFlag.None,
            SpaceTag = TagFlag.Eval,
        });
    }

    public void AddStyleVarsWithReplacement(string styleVar, string replacement, bool preserve)
    {
        AddReplacement(ref _valueByStyleVar, styleVar, replacement, preserve);
    }

    public void AddStyleConstantVarsWithReplacement(string styleConstantVar, string replacement, bool preserve)
    {
        AddReplacement(ref _valueByStyleConstantVar, styleConstantVar, replacement, preserve);
    }

    // kept here instead of read from the CultureInfo, so finding the scaling tags doesn't need to load any culture data
    internal static char GetDecimalSeparator(StormLocale stormLocale) => stormLocale switch
    {
        StormLocale.DEDE or
        StormLocale.ESES or
        StormLocale.FRFR or
        StormLocale.ITIT or
        StormLocale.PLPL or
        StormLocale.PTBR or
        StormLocale.RURU => ',',

        _ => '.',
    };

    private static void AddReplacement(ref ReplacementLookup? replacements, string value, string replacement, bool preserve)
    {
        replacements ??= new Dictionary<string, (string Value, bool Preserve)>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
        replacements.Value.Dictionary[value] = (replacement, preserve);
    }

    private static void CopyIntoBuffer(Span<char> buffer, ref int offset, ReadOnlySpan<char> item, bool cleanTheTag)
    {
        item.CopyTo(buffer[offset..]);

        if (cleanTheTag)
            offset += CleanUpTag(buffer.Slice(offset, item.Length));
        else
            offset += item.Length;
    }

    // replaces double space or more into single space, and lowercases the tag type
    private static int CleanUpTag(Span<char> text)
    {
        int writeIndex = 0;

        for (int position = 0; position < text.Length; position++)
        {
            if (text[position] == ' ' && writeIndex > 0 && text[writeIndex - 1] == ' ')
                continue; // skip for second consecutive space

            text[writeIndex++] = text[position];
        }

        text = text[..writeIndex];

        Span<char> tagTypeSpan;
        int spaceIndex = text.IndexOf(' ');

        if (spaceIndex > -1)
            tagTypeSpan = text[..spaceIndex].TrimStart('<');
        else
            tagTypeSpan = text.Trim("</>");

        for (int j = 0; j < tagTypeSpan.Length; j++)
            tagTypeSpan[j] = char.ToLowerInvariant(tagTypeSpan[j]);

        return text.Length;
    }

    // checks if the end tag matches the start tag
    private static bool IsTagsMatch(ReadOnlySpan<char> startTag, ReadOnlySpan<char> endTag)
    {
        // strips both the < and >, so tags with no attributes (e.g. <c>) also match
        return GetEndTagCharType(startTag).Equals(endTag.Trim("</> "), StringComparison.OrdinalIgnoreCase);
    }

    private static ReadOnlySpan<char> GetEndTagCharType(ReadOnlySpan<char> startTagText)
    {
        // remove the < and > from the start tag
        ReadOnlySpan<char> inner = startTagText[1..^1];

        int spaceIndex = inner.IndexOf(' ');
        return spaceIndex > -1 ? inner[..spaceIndex] : inner;
    }

    private static bool IsNewLineTag(ReadOnlySpan<char> tag) => tag.Equals("<n/>", StringComparison.OrdinalIgnoreCase) || tag.Equals("</n>", StringComparison.OrdinalIgnoreCase) || tag.Equals("<n>", StringComparison.OrdinalIgnoreCase);

    private static bool IsSpaceTag(ReadOnlySpan<char> tag) => tag.Equals("<sp/>", StringComparison.OrdinalIgnoreCase) || tag.Equals("</sp>", StringComparison.OrdinalIgnoreCase) || tag.Equals("<sp>", StringComparison.OrdinalIgnoreCase);

    private static bool IsSelfCloseTag(ReadOnlySpan<char> tag)
    {
        // <li> tags are a thing inside of an <ol> or <ul>, but since no tooltips actually use them so far we can ignore them
        return !tag.Equals("<li/>", StringComparison.OrdinalIgnoreCase) && tag.Length > 3 && tag.EndsWith("/>", StringComparison.OrdinalIgnoreCase);
    }

    // <s ...> is a style tag and <c ...> is a constant tag
    private static bool TryGetFontTagType(ReadOnlySpan<char> tag, out FontTagType fontTagType)
    {
        if (tag.StartsWith("<s", StringComparison.OrdinalIgnoreCase))
        {
            fontTagType = FontTagType.Style;
            return true;
        }

        if (tag.StartsWith("<c", StringComparison.OrdinalIgnoreCase))
        {
            fontTagType = FontTagType.Constant;
            return true;
        }

        fontTagType = default;
        return false;
    }

    // digits with at most one decimal separator, which can be a period or the locale's decimal separator
    private static bool IsDigits(ReadOnlySpan<char> value, char decimalSeparator)
    {
        // whitespace doesn't need its own check, the loop rejects it
        if (value.IsEmpty)
            return false;

        bool hasSeparator = false;

        foreach (char c in value)
        {
            if (char.IsAsciiDigit(c))
                continue;

            if (hasSeparator || (c != '.' && c != decimalSeparator))
                return false;

            hasSeparator = true;
        }

        return true;
    }

    // the value already passed IsDigits, so a comma can only be the decimal separator
    private static bool TryParseScalingValue(ReadOnlySpan<char> value, out double result)
    {
        NumberFormatInfo numberFormat = value.Contains(',') ? _commaDecimalNumberFormat : NumberFormatInfo.InvariantInfo;

        return double.TryParse(value, NumberStyles.AllowDecimalPoint, numberFormat, out result);
    }

    private static ReadOnlySpan<char> GetFontTagVal(ReadOnlySpan<char> tag)
    {
        return GetAttributeValue(tag, "val=");
    }

    private static ReadOnlySpan<char> GetAttributeValue(ReadOnlySpan<char> tag, ReadOnlySpan<char> attributeName)
    {
        // <c val=\"#TooltipNumbers\">
        // <s val=\"StandardTooltipHeader\">
        int indexOfAttributeName = tag.IndexOf(attributeName, StringComparison.OrdinalIgnoreCase);
        if (indexOfAttributeName < 0)
            return [];

        ReadOnlySpan<char> attributeNameSpan = tag[indexOfAttributeName..];

        int startIndexOfQuote = attributeNameSpan.IndexOf('"');
        int endIndexOfQuote = attributeNameSpan[(startIndexOfQuote + 1)..].IndexOf('"');

        if (endIndexOfQuote < 1)
            return []; // no end quote found, return empty

        return attributeNameSpan[(startIndexOfQuote + 1)..(startIndexOfQuote + endIndexOfQuote + 1)].Trim();
    }

    // finds the inside of the val attribute's quotes, skipping over the hlt-name attribute
    private static bool TryGetValValueRange(ReadOnlySpan<char> startTag, int hltNameStart, int hltNameEnd, out int valueStart, out int valueEnd)
    {
        valueStart = 0;
        valueEnd = 0;

        int indexOfVal = startTag[..hltNameStart].IndexOf("val=", StringComparison.OrdinalIgnoreCase);
        if (indexOfVal < 0)
        {
            indexOfVal = startTag[hltNameEnd..].IndexOf("val=", StringComparison.OrdinalIgnoreCase);
            if (indexOfVal < 0)
                return false;

            indexOfVal += hltNameEnd;
        }

        int startIndexOfQuote = startTag[indexOfVal..].IndexOf('"');
        if (startIndexOfQuote < 0)
            return false;

        valueStart = indexOfVal + startIndexOfQuote + 1;

        int endIndexOfQuote = startTag[valueStart..].IndexOf('"');
        if (endIndexOfQuote < 0)
            return false;

        valueEnd = valueStart + endIndexOfQuote;

        // an unquoted val could pick up the quotes of the hlt-name attribute
        return valueEnd <= hltNameStart || valueStart >= hltNameEnd;
    }

    private static void CopyEndTag(Span<char> buffer, ref int offset, ReadOnlySpan<char> tagType)
    {
        buffer[offset++] = '<';
        buffer[offset++] = '/';

        // lowercase to match the start tag, which gets lowercased by CleanUpTag
        offset += tagType.ToLowerInvariant(buffer[offset..]);
        buffer[offset++] = '>';
    }

    private string BuildDescription(GameStringFlags flags)
    {
        EnsureParsed();

        if (_isPlainText)
            return _description;

        if (_textStack.Count < 1)
            return string.Empty;

        ReadOnlySpan<char> gameString = _description;
        int totalSize = GetSizeOfBuffer(gameString, flags);

        char[]? rentedBuffer = null;
        Span<char> buffer = totalSize < 1024 ? stackalloc char[totalSize] : (rentedBuffer = ArrayPool<char>.Shared.Rent(totalSize));

        // a start tag is only copied in once there is text for it, so empty tags are dropped
        ReadOnlySpan<char> startTag = [];
        int currentOffset = 0;

        foreach (TextRange item in CollectionsMarshal.AsSpan(_textStack))
        {
            ReadOnlySpan<char> itemText = item.GetText(gameString);

            switch (item.Type)
            {
                case TextType.Newline:
                    if (flags.NewLineTag == TagFlag.Include)
                        CopyIntoBuffer(buffer, ref currentOffset, "<n/>", false);
                    else if (flags.NewLineTag == TagFlag.Eval)
                        CopyIntoBuffer(buffer, ref currentOffset, " ", false);
                    break;
                case TextType.SpaceTag:
                    if (flags.SpaceTag == TagFlag.Include)
                        CopyIntoBuffer(buffer, ref currentOffset, itemText, false);
                    else if (flags.SpaceTag == TagFlag.Eval)
                        CopyIntoBuffer(buffer, ref currentOffset, " ", false);
                    break;
                case TextType.StartTag:
                    startTag = itemText;
                    break;
                case TextType.MissingEndTag:
                case TextType.EndTag:
                    if (flags.ColorTags == TagFlag.Include)
                    {
                        if (startTag.IsEmpty)
                        {
                            if (item.Type == TextType.EndTag)
                                CopyIntoBuffer(buffer, ref currentOffset, itemText, true);
                            else
                                CopyEndTag(buffer, ref currentOffset, GetEndTagCharType(itemText));
                        }
                        else
                        {
                            // dont save, empty tag
                            startTag = [];
                        }
                    }

                    break;
                case TextType.ScalingTag:
                    if (flags.ScalingTag != TagFlag.None && flags.ColorTags == TagFlag.Include && !startTag.IsEmpty)
                    {
                        CopyStartTag(buffer, startTag, ref currentOffset);
                        startTag = [];
                    }

                    if (flags.ScalingTag == TagFlag.Eval && TryParseScalingValue(itemText.Trim('~'), out double scaleValue))
                        CopyScalingText(buffer, ref currentOffset, scaleValue);
                    else if (flags.ScalingTag == TagFlag.Include)
                        CopyIntoBuffer(buffer, ref currentOffset, itemText, false);

                    break;
                case TextType.SelfCloseTag:
                    if (flags.ColorTags == TagFlag.Include)
                        CopyIntoBuffer(buffer, ref currentOffset, itemText, false);

                    break;
                default:
                    if (flags.ColorTags == TagFlag.Include && !startTag.IsEmpty)
                    {
                        CopyStartTag(buffer, startTag, ref currentOffset);
                        startTag = [];

                        CopyIntoBuffer(buffer, ref currentOffset, itemText, false);
                    }
                    else if (item.Type != TextType.ErrorTag || flags.ErrorTag == TagFlag.Include)
                    {
                        CopyIntoBuffer(buffer, ref currentOffset, itemText, false);
                    }

                    break;
            }
        }

        // slice, so no null chars the end
        ReadOnlySpan<char> result = buffer[..currentOffset];

        // reuse the original string if nothing changed, saves an allocation
        string description = result.SequenceEqual(gameString) ? _description : result.ToString();

        if (rentedBuffer is not null)
            ArrayPool<char>.Shared.Return(rentedBuffer);

        return description;
    }

    private void ConstructTextStack(ReadOnlySpan<char> gameString, Range? startTag = null)
    {
        _startingIndex = _index;

        while (_index < gameString.Length)
        {
            // skip to the next special character
            int nextSpecialIndex = gameString[_index..].IndexOfAny(_specialChars);
            if (nextSpecialIndex < 0)
            {
                _index = gameString.Length;

                break;
            }

            _index += nextSpecialIndex;

            if (gameString[_index] == '<' && _index + 1 < gameString.Length && gameString[_index + 1] != ' ')
            {
                bool pushedText = PushNormalText();

                if (TryParseTag(gameString, out Range tag, out bool isStartTag))
                {
                    ReadOnlySpan<char> tagText = gameString[tag];

                    if (IsNewLineTag(tagText))
                    {
                        // nested, so the outer tag is closed before the newline and opened again after it
                        if (startTag.HasValue)
                        {
                            PushOuterTagEnd(gameString, startTag.Value);

                            _textStack.Add(new TextRange(tag, TextType.Newline));
                            _textStack.Add(new TextRange(startTag.Value, TextType.StartTag));
                        }
                        else
                        {
                            _textStack.Add(new TextRange(tag, TextType.Newline));
                        }
                    }
                    else if (IsSpaceTag(tagText))
                    {
                        _textStack.Add(new TextRange(tag, TextType.SpaceTag));
                    }
                    else if (isStartTag)
                    {
                        PushFontVarFromTag(tagText);

                        // nested, so the outer tag is closed before the inner tag and opened again after it
                        if (startTag.HasValue)
                            PushOuterTagEnd(gameString, startTag.Value);

                        _textStack.Add(new TextRange(tag, TextType.StartTag));

                        ConstructTextStack(gameString, tag);

                        if (startTag.HasValue)
                            _textStack.Add(new TextRange(startTag.Value, TextType.StartTag));
                    }
                    else if (startTag.HasValue)
                    {
                        if (IsTagsMatch(gameString[startTag.Value], tagText))
                        {
                            _textStack.Add(new TextRange(tag, TextType.EndTag));

                            return;
                        }
                    }
                    else if (IsSelfCloseTag(tagText))
                    {
                        _textStack.Add(new TextRange(tag, TextType.SelfCloseTag));
                    }
                }
                else
                {
                    // not a tag, so take back the text and push it again with the < included
                    if (pushedText)
                        _textStack.RemoveAt(_textStack.Count - 1);

                    PushNormalText();
                }

                _startingIndex = _index;
            }
            else if (gameString[_index] == '~' && _index + 1 < gameString.Length && gameString[_index + 1] == '~')
            {
                bool pushedText = PushNormalText();

                if (TryParseScalingTag(gameString, out Range tag))
                {
                    _textStack.Add(new TextRange(tag, TextType.ScalingTag));

                    _startingIndex = _index;
                }
                else if (pushedText)
                {
                    // not a scaling tag, so take back the text and let it carry on past the ~~
                    _textStack.RemoveAt(_textStack.Count - 1);
                }
            }
            else if (gameString[_index] == '#' && _index + 1 < gameString.Length && gameString[_index + 1] == '#')
            {
                bool pushedText = PushNormalText();

                if (TryParseErrorTag(gameString, out Range tag))
                {
                    _textStack.Add(new TextRange(tag, TextType.ErrorTag));

                    _startingIndex = _index;
                }
                else if (pushedText)
                {
                    // not an error tag, so take back the text and let it carry on past the ##
                    _textStack.RemoveAt(_textStack.Count - 1);
                }
            }
            else if (gameString[_index] == '%' && _index == _startingIndex && _textStack.Count > 0 && _textStack[^1].Type == TextType.ScalingTag)
            {
                // only when the percent is right after the scaling tag, otherwise the text in between would get moved too
                // push the percent as text, then swap it in front of the scaling tag
                _index++;

                PushNormalText();

                (_textStack[^1], _textStack[^2]) = (_textStack[^2], _textStack[^1]);

                _startingIndex = _index;
            }
            else
            {
                _index++;
            }
        }

        // reached the end of the string without finding the matching end tag (that case returns early)
        PushNormalText();

        if (startTag.HasValue)
            _textStack.Add(new TextRange(startTag.Value, TextType.MissingEndTag));
    }

    // pushes the text from the starting index up to the current index, returns false if there is no text
    private bool PushNormalText()
    {
        if (_index <= _startingIndex)
            return false;

        _textStack.Add(new TextRange(_startingIndex, _index, TextType.Text));
        return true;
    }

    // closes the outer tag with its end tag, or marks it as missing one
    private void PushOuterTagEnd(ReadOnlySpan<char> gameString, Range outerStartTag)
    {
        if (TryGetEndTag(gameString, outerStartTag, out Range endTag))
            _textStack.Add(new TextRange(endTag, TextType.EndTag));
        else
            _textStack.Add(new TextRange(outerStartTag, TextType.MissingEndTag));
    }

    // the current index is at a <
    private bool TryParseTag(ReadOnlySpan<char> gameString, out Range tag, out bool isStartTag)
    {
        // the next < or > after the starting <
        int nextBracketIndex = gameString[(_index + 1)..].IndexOfAny('<', '>') + 1;

        if (nextBracketIndex > 0 && gameString[_index + nextBracketIndex] == '>')
        {
            tag = new Range(_index, _index + nextBracketIndex + 1);

            ReadOnlySpan<char> tagText = gameString[tag];
            isStartTag = tagText[1] != '/' && tagText[^2] != '/';

            _index += nextBracketIndex + 1;

            return true;
        }

        tag = default;
        isStartTag = false;

        // not a tag, so carry on from the next < if there is one
        _index += nextBracketIndex > 0 ? nextBracketIndex : 1;

        return false;
    }

    // the current index is at the first ~ of a ~~
    private bool TryParseScalingTag(ReadOnlySpan<char> gameString, out Range tag)
    {
        int valueStart = _index + 2;
        int valueLength = gameString[valueStart..].IndexOf("~~");

        if (valueLength >= 0 && IsDigits(gameString.Slice(valueStart, valueLength), _decimalSeparator))
        {
            int tagEnd = valueStart + valueLength + 2;

            tag = new Range(_index, tagEnd);
            _index = tagEnd;

            return true;
        }

        tag = default;
        _index += 2;

        return false;
    }

    // the current index is at the first # of a ##
    private bool TryParseErrorTag(ReadOnlySpan<char> gameString, out Range tag)
    {
        if (gameString[_index..].StartsWith(GameStringText.ErrorTag, StringComparison.Ordinal))
        {
            int tagEnd = _index + GameStringText.ErrorTag.Length;

            tag = new Range(_index, tagEnd);
            _index = tagEnd;

            return true;
        }

        tag = default;
        _index += 2;

        return false;
    }

    // find the index of the end tag
    private bool TryGetEndTag(ReadOnlySpan<char> gameString, Range startTag, out Range endTag)
    {
        ReadOnlySpan<char> currentTextSpan = gameString[_index..];

        // find the tag type
        ReadOnlySpan<char> tagTypeSpan = GetEndTagCharType(gameString[startTag]);

        int searchLength = tagTypeSpan.Length + 3; // </ + tagType + >

        for (int i = 0; i <= currentTextSpan.Length - searchLength; i++)
        {
            int next = currentTextSpan[i..].IndexOf('<');

            if (next < 0)
                break;

            i += next;

            if (i > currentTextSpan.Length - searchLength)
                break;

            if (currentTextSpan[i + 1] == '/' &&
                currentTextSpan[i + searchLength - 1] == '>' &&
                currentTextSpan.Slice(i + 2, tagTypeSpan.Length).Equals(tagTypeSpan, StringComparison.OrdinalIgnoreCase))
            {
                endTag = new Range(_index + i, _index + i + searchLength);
                return true;
            }
        }

        endTag = default;
        return false;
    }

    private void PushFontVarFromTag(ReadOnlySpan<char> tag)
    {
        if (!ExtractFontVars || !TryGetFontTagType(tag, out FontTagType fontTagType))
            return;

        // <c val=\"#TooltipNumbers\">
        // <s val=\"StandardTooltipHeader\">
        ReadOnlySpan<char> fontTagVal = GetFontTagVal(tag);
        if (fontTagVal.IsEmpty)
            return;

        HashSet<string> fontVars = fontTagType == FontTagType.Style ? _styleTagVariables : _styleConstantTagVariables;

        fontVars.GetAlternateLookup<ReadOnlySpan<char>>().Add(fontTagVal);
    }

    private ReplacementLookup? GetReplacements(FontTagType fontTagType) => fontTagType == FontTagType.Style ? _valueByStyleVar : _valueByStyleConstantVar;

    private bool IsHltNameRemoved(FontTagType fontTagType) => fontTagType == FontTagType.Style ? RemoveHltNameFromStyleTags : RemoveHltNameFromConstantTags;

    private bool IsHltNameReplacementUndone(FontTagType fontTagType) => fontTagType == FontTagType.Style ? UndoHltNameReplacementForStyleTags : UndoHltNameReplacementForConstantTags;

    // copies in the start tag with its font value changes, or cleaned up as-is if it has none
    private void CopyStartTag(Span<char> buffer, ReadOnlySpan<char> startTag, ref int currentOffset)
    {
        if (!FontValueCopiedInBuffer(buffer, startTag, ref currentOffset))
            CopyIntoBuffer(buffer, ref currentOffset, startTag, true);
    }

    private bool FontValueCopiedInBuffer(Span<char> buffer, ReadOnlySpan<char> startTag, ref int currentOffset)
    {
        if (!TryGetFontTagType(startTag, out FontTagType fontTagType))
            return false;

        // only the tag type that has hlt-name removal turned on is affected, the other tag type still gets its value replacements
        if (IsHltNameRemoved(fontTagType))
        {
            // removing the hlt-name never makes the tag longer, so it fits in a buffer the length of the tag
            char[]? rentedBuffer = null;
            Span<char> tagBuffer = startTag.Length <= 256 ? stackalloc char[256] : (rentedBuffer = ArrayPool<char>.Shared.Rent(startTag.Length));

            int tagLength = 0;
            bool isHltNameRemoved = HltNameRemovedInBuffer(tagBuffer, startTag, fontTagType, ref tagLength);

            if (isHltNameRemoved)
            {
                // the hlt-name removal goes first, then the replacement is for the val that is left
                ReadOnlySpan<char> tagWithoutHltName = tagBuffer[..tagLength];

                if (!CopiedInFontTagVal(buffer, tagWithoutHltName, fontTagType, ref currentOffset))
                    CopyIntoBuffer(buffer, ref currentOffset, tagWithoutHltName, false);
            }

            if (rentedBuffer is not null)
                ArrayPool<char>.Shared.Return(rentedBuffer);

            if (isHltNameRemoved)
                return true;
        }

        return CopiedInFontTagVal(buffer, startTag, fontTagType, ref currentOffset);
    }

    // the val value that a replacement is looked up by, which is the hlt-name value when the hlt-name removal undoes the replacement
    private ReadOnlySpan<char> GetFontTagValForReplacement(ReadOnlySpan<char> startTag, FontTagType fontTagType)
    {
        if (IsHltNameRemoved(fontTagType) && IsHltNameReplacementUndone(fontTagType))
        {
            ReadOnlySpan<char> hltNameVal = GetAttributeValue(startTag, _hltNameAttribute);

            if (!hltNameVal.IsEmpty)
                return hltNameVal;
        }

        return GetFontTagVal(startTag);
    }

    // removes the hlt-name attribute, and if undoing the replacement then sets the val attribute back to the hlt-name value
    // the attributes can be in any order, and the other attributes are kept
    // <s val=\"123456\" hlt-name=\"StandardTooltipHeader\" otherAtt=\"somevalue\" >
    private bool HltNameRemovedInBuffer(Span<char> buffer, ReadOnlySpan<char> startTag, FontTagType fontTagType, ref int currentOffset)
    {
        int indexOfHltName = startTag.IndexOf(_hltNameAttribute, StringComparison.OrdinalIgnoreCase);
        if (indexOfHltName < 0)
            return false;

        ReadOnlySpan<char> hltNameSpan = startTag[indexOfHltName..];

        int startIndexOfQuote = hltNameSpan.IndexOf('"');
        int endIndexOfQuote = hltNameSpan[(startIndexOfQuote + 1)..].IndexOf('"');

        if (endIndexOfQuote < 0)
            return false;

        // the part that gets removed, which is the hlt-name attribute and the whitespace before it
        int hltNameStart = startTag[..indexOfHltName].TrimEnd().Length;
        int hltNameEnd = indexOfHltName + startIndexOfQuote + endIndexOfQuote + 2;

        int tagStartOffset = currentOffset;

        if (IsHltNameReplacementUndone(fontTagType))
        {
            if (!TryGetValValueRange(startTag, hltNameStart, hltNameEnd, out int valueStart, out int valueEnd))
                return false;

            ReadOnlySpan<char> hltNameValue = hltNameSpan[(startIndexOfQuote + 1)..(startIndexOfQuote + endIndexOfQuote + 1)].Trim();

            // the val value is swapped for the hlt-name value
            if (valueStart < hltNameStart)
            {
                CopyIntoBuffer(buffer, ref currentOffset, startTag[..valueStart], false);
                CopyIntoBuffer(buffer, ref currentOffset, hltNameValue, false);
                CopyIntoBuffer(buffer, ref currentOffset, startTag[valueEnd..hltNameStart], false);
                CopyIntoBuffer(buffer, ref currentOffset, startTag[hltNameEnd..], false);
            }
            else
            {
                CopyIntoBuffer(buffer, ref currentOffset, startTag[..hltNameStart], false);
                CopyIntoBuffer(buffer, ref currentOffset, startTag[hltNameEnd..valueStart], false);
                CopyIntoBuffer(buffer, ref currentOffset, hltNameValue, false);
                CopyIntoBuffer(buffer, ref currentOffset, startTag[valueEnd..], false);
            }
        }
        else
        {
            CopyIntoBuffer(buffer, ref currentOffset, startTag[..hltNameStart], false);
            CopyIntoBuffer(buffer, ref currentOffset, startTag[hltNameEnd..], false);
        }

        // cleaned as a whole, so any spaces left around the removed attribute get collapsed
        currentOffset = tagStartOffset + CleanUpTag(buffer[tagStartOffset..currentOffset]);

        return true;
    }

    // for copy in the replacement value of the font tag, and if preserve is true then also copy in the hlt-name attribute with the original value (unless the tag already has one)
    private bool CopiedInFontTagVal(Span<char> buffer, ReadOnlySpan<char> startTag, FontTagType fontTagType, ref int currentOffset)
    {
        // checked first, so the val isn't searched for when there are no replacements
        if (GetReplacements(fontTagType) is not { } replacements)
            return false;

        // "<s val=\"StandardTooltipHeader\">"
        ReadOnlySpan<char> fontTagVal = GetFontTagVal(startTag);

        if (fontTagVal.IsEmpty || !replacements.TryGetValue(fontTagVal, out (string Value, bool Preserve) newValue))
            return false;

        // fontTagVal is a slice of startTag, so get its exact position.
        startTag.Overlaps(fontTagVal, out int indexOfStyleVar);

        // the closing quote of the val attribute, the tag is kept from there on (e.g. other attributes)
        int indexOfEndQuote = indexOfStyleVar + fontTagVal.Length;
        indexOfEndQuote += startTag[indexOfEndQuote..].IndexOf('"');

        int tagStartOffset = currentOffset;

        // <s val=\"
        CopyIntoBuffer(buffer, ref currentOffset, startTag[..indexOfStyleVar], false);
        CopyIntoBuffer(buffer, ref currentOffset, newValue.Value, false);

        // a tag that already has an hlt-name keeps it instead, since it already has the original value
        if (newValue.Preserve && !startTag.Contains(_hltNameAttribute, StringComparison.OrdinalIgnoreCase))
        {
            // $"\" {_hltNameAttribute}\"{fontTagVal}"
            buffer[currentOffset++] = '"';
            buffer[currentOffset++] = ' ';
            CopyIntoBuffer(buffer, ref currentOffset, _hltNameAttribute, false);
            buffer[currentOffset++] = '"';
            CopyIntoBuffer(buffer, ref currentOffset, fontTagVal, false);
        }

        // \"> or \" otherAtt=\"somevalue\">
        CopyIntoBuffer(buffer, ref currentOffset, startTag[indexOfEndQuote..], false);

        // cleaned as a whole, the same as a tag with no replacement
        currentOffset = tagStartOffset + CleanUpTag(buffer[tagStartOffset..currentOffset]);

        return true;
    }

    private int GetSizeOfBuffer(ReadOnlySpan<char> gameString, GameStringFlags flags)
    {
        int sum = 0;

        foreach (TextRange current in CollectionsMarshal.AsSpan(_textStack))
        {
            switch (current.Type)
            {
                case TextType.MissingEndTag:
                    sum += GetEndTagCharType(current.GetText(gameString)).Length + 3; // 3 for </>
                    break;
                case TextType.Newline:
                    if (flags.NewLineTag == TagFlag.Include)
                        sum += 4; // <n/>
                    else if (flags.NewLineTag == TagFlag.Eval)
                        sum += 1; // as space " "
                    break;
                case TextType.SpaceTag:
                    if (flags.SpaceTag == TagFlag.Include)
                        sum += 5; // <sp/>
                    else if (flags.SpaceTag == TagFlag.Eval)
                        sum += 1; // as space " "
                    break;
                case TextType.StartTag:
                    if (flags.ColorTags == TagFlag.Include)
                        sum += current.Length + GetFontValueReplacementGrowth(current.GetText(gameString));
                    break;
                case TextType.EndTag:
                    if (flags.ColorTags == TagFlag.Include)
                        sum += current.Length;
                    break;
                case TextType.ScalingTag:
                    if (flags.ScalingTag == TagFlag.Include)
                        sum += current.Length;
                    else if (flags.ScalingTag == TagFlag.Eval)
                        sum += 21 + current.Length; // 21 largest min text
                    break;
                default:
                    sum += current.Length;
                    break;
            }
        }

        return sum;
    }

    // the most that a start tag can grow by from a font value replacement
    private int GetFontValueReplacementGrowth(ReadOnlySpan<char> startTag)
    {
        if (!TryGetFontTagType(startTag, out FontTagType fontTagType) || GetReplacements(fontTagType) is not { } replacements)
            return 0;

        ReadOnlySpan<char> fontTagVal = GetFontTagValForReplacement(startTag, fontTagType); // e.g. #TooltipNumbers

        if (!replacements.TryGetValue(fontTagVal, out (string Value, bool Preserve) newValue))
            return 0;

        int growth = Math.Max(0, newValue.Value.Length - fontTagVal.Length);

        // hlt-name="#TooltipNumbers" +3 for the space and quotes
        if (newValue.Preserve)
            growth += 3 + _hltNameAttribute.Length + fontTagVal.Length;

        return growth;
    }

    private void CopyScalingText(Span<char> buffer, ref int offset, double value)
    {
        // formatted the same way as the gamestrings:
        // dede and frfr have a non-breaking space ( ) before the %,
        // zhcn and zhtw use full-width parentheses with no space before them
        ReadOnlySpan<char> format = _gameStringLocale switch
        {
            StormLocale.ENUS => " (+0.##% per level)",
            StormLocale.DEDE => " (+0.## % pro Stufe)",
            StormLocale.ESES => " (+0.##% por nivel)",
            StormLocale.ESMX => " (+0.##% por nivel)",
            StormLocale.FRFR => " (+0.## % par niveau)",
            StormLocale.ITIT => " (+0.##% per livello)",
            StormLocale.KOKR => " (레벨당 +0.##%)",
            StormLocale.PLPL => " (+0.##% na poziom)",
            StormLocale.PTBR => " (+0.##% por nível)",
            StormLocale.RURU => " (+0.##% за уровень)",
            StormLocale.ZHCN => "（每级+0.##%）",
            StormLocale.ZHTW => "（每級+0.##%）",

            _ => " (+0.##% per level)",
        };

        value.TryFormat(buffer[offset..], out int charsWritten, format, StormLocaleCulture.GetCultureInfo(_gameStringLocale));

        offset += charsWritten;
    }
}
