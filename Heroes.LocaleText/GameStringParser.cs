using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    private Dictionary<string, (string Value, bool Preserve)>? _valueByStyleVar;
    private Dictionary<string, (string Value, bool Preserve)>? _valueByStyleConstantVar;

#if NET9_0_OR_GREATER
    private Dictionary<string, (string Value, bool Preserve)>.AlternateLookup<ReadOnlySpan<char>>? _valueByStyleVarAltLookup;
    private Dictionary<string, (string Value, bool Preserve)>.AlternateLookup<ReadOnlySpan<char>>? _valueByStyleConstantVarAltLookup;
#endif

    private bool _isConstructed = false;
    private bool _isPlainText = false;
    private int _startingIndex = 0;
    private int _index = 0;

    private GameStringParser(string description, StormLocale gameStringLocale, bool extractFontVars)
    {
        _description = description;
        _gameStringLocale = gameStringLocale;
        _decimalSeparator = GetDecimalSeparator(gameStringLocale);

        ExtractFontVars = extractFontVars;
        if (ExtractFontVars is true)
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
        EnsureTextStackConstructed(_description);
    }

    public string GetRawText()
    {
        return Parse(_description);
    }

    public string GetPlainText(bool includeNewLineTags, bool includeScaling)
    {
        return ParseToPlainText(_description, includeNewLineTags, includeScaling);
    }

    public string GetColoredText(bool includeScaling)
    {
        return ParseToColoredText(_description, includeScaling);
    }

    public void AddStyleVarsWithReplacement(string styleVar, string replacement, bool preserve)
    {
        _valueByStyleVar ??= new(StringComparer.Ordinal);
#if NET9_0_OR_GREATER
        _valueByStyleVarAltLookup ??= _valueByStyleVar.GetAlternateLookup<ReadOnlySpan<char>>();
#endif
        _valueByStyleVar[styleVar] = (replacement, preserve);
    }

    public void AddStyleConstantVarsWithReplacement(string styleConstantVar, string replacement, bool preserve)
    {
        _valueByStyleConstantVar ??= new(StringComparer.Ordinal);
#if NET9_0_OR_GREATER
        _valueByStyleConstantVarAltLookup ??= _valueByStyleConstantVar.GetAlternateLookup<ReadOnlySpan<char>>();
#endif
        _valueByStyleConstantVar[styleConstantVar] = (replacement, preserve);
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
    private static bool IsTagsMatch(ReadOnlySpan<char> text, Range startTag, Range endTag)
    {
        ReadOnlySpan<char> endSpan = text[endTag].Trim("</> ");

        // strips both the < and >, so tags with no attributes (e.g. <c>) also match
        ReadOnlySpan<char> startTagType = GetEndTagCharType(text[startTag]);

        return startTagType.Equals(endSpan, StringComparison.OrdinalIgnoreCase);
    }

    private static ReadOnlySpan<char> GetEndTagCharType(ReadOnlySpan<char> startTagText)
    {
        // remove the < and > from the start tag
        ReadOnlySpan<char> inner = startTagText[1..^1];

        int spaceIndex = inner.IndexOf(' ');
        return spaceIndex > -1 ? inner[..spaceIndex] : inner;
    }

    private static bool IsNewLineTag(ReadOnlySpan<char> text, Range tag) => IsNewLineTag(text[tag]);

    private static bool IsNewLineTag(ReadOnlySpan<char> text) => text.Equals("<n/>", StringComparison.OrdinalIgnoreCase) || text.Equals("</n>", StringComparison.OrdinalIgnoreCase) || text.Equals("<n>", StringComparison.OrdinalIgnoreCase);

    private static bool IsSpaceTag(ReadOnlySpan<char> text, Range tag) => IsSpaceTag(text[tag]);

    private static bool IsSpaceTag(ReadOnlySpan<char> text) => text.Equals("<sp/>", StringComparison.OrdinalIgnoreCase) || text.Equals("</sp>", StringComparison.OrdinalIgnoreCase) || text.Equals("<sp>", StringComparison.OrdinalIgnoreCase);

    private static bool IsSelfCloseTag(ReadOnlySpan<char> text, Range tag)
    {
        ReadOnlySpan<char> tagSpan = text[tag];

        // <li> tags are a thing inside of an <ol> or <ul>, but since no tooltips acutally use them so far we can ignore them
        return !tagSpan.Equals("<li/>", StringComparison.OrdinalIgnoreCase) && tagSpan.Length > 3 && tagSpan.EndsWith("/>", StringComparison.OrdinalIgnoreCase);
    }

    // digits with at most one decimal separator, which can be a period or the locale's decimal separator
    private static bool IsDigits(ReadOnlySpan<char> value, char decimalSeparator)
    {
        // whitespace doesn't need its own check, the loop rejects it
        if (value.IsEmpty)
            return false;

        bool hasSeparator = false;

        for (int i = 0; i < value.Length; i++)
        {
            if ((uint)(value[i] - '0') <= 9)
                continue;
            else if (hasSeparator is false && (value[i] == '.' || value[i] == decimalSeparator))
                hasSeparator = true;
            else
                return false;
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

    private static void AddFontVar(HashSet<string> fontVars, ReadOnlySpan<char> fontTagVal)
    {
#if NET9_0_OR_GREATER
        // only allocates a string if the value is not already in the set
        fontVars.GetAlternateLookup<ReadOnlySpan<char>>().Add(fontTagVal);
#else
        fontVars.Add(fontTagVal.ToString());
#endif
    }

    private string Parse(ReadOnlySpan<char> gameString)
    {
        EnsureTextStackConstructed(gameString);

        return BuildDescription(gameString, new GameStringFlags()
        {
            ColorTags = TagFlag.Include,
            ScalingTag = TagFlag.Include,
            NewLineTag = TagFlag.Include,
            ErrorTag = TagFlag.Include,
            SpaceTag = TagFlag.Include,
        });
    }

    private string ParseToPlainText(ReadOnlySpan<char> gameString, bool includeNewlineTags, bool includeScaling)
    {
        EnsureTextStackConstructed(gameString);

        return BuildDescription(gameString, new GameStringFlags()
        {
            ColorTags = TagFlag.None,
            ScalingTag = includeScaling ? TagFlag.Eval : TagFlag.None,
            NewLineTag = includeNewlineTags ? TagFlag.Include : TagFlag.Eval,
            ErrorTag = TagFlag.None,
            SpaceTag = TagFlag.Eval,
        });
    }

    private string ParseToColoredText(ReadOnlySpan<char> gameString, bool includeScaling)
    {
        EnsureTextStackConstructed(gameString);

        return BuildDescription(gameString, new GameStringFlags()
        {
            ColorTags = TagFlag.Include,
            ScalingTag = includeScaling ? TagFlag.Eval : TagFlag.None,
            NewLineTag = TagFlag.Include,
            ErrorTag = TagFlag.None,
            SpaceTag = TagFlag.Eval,
        });
    }

    private void EnsureTextStackConstructed(ReadOnlySpan<char> gameString)
    {
        if (_isConstructed)
            return;

        _isConstructed = true;

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

    private string BuildDescription(ReadOnlySpan<char> gameString, GameStringFlags flags)
    {
        if (_isPlainText)
            return _description;

        if (_textStack.Count < 1)
            return string.Empty;

        int totalSize = GetSizeOfBuffer(gameString, flags);

        char[]? rentedBuffer = null;
        Span<char> buffer = totalSize < 1024 ? stackalloc char[totalSize] : (rentedBuffer = ArrayPool<char>.Shared.Rent(totalSize));

        ReadOnlySpan<char> startTag = [];
        int currentOffset = 0;

        // loop through and build string
        ReadOnlySpan<TextRange> textStack = CollectionsMarshal.AsSpan(_textStack);
        for (int i = 0; i < textStack.Length; i++)
        {
            TextRange item = textStack[i];
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
                    {
                        if (flags.ScalingTag != TagFlag.None && flags.ColorTags == TagFlag.Include && !startTag.IsEmpty)
                        {
                            if (!FontValueCopiedInBuffer(buffer, startTag, ref currentOffset))
                                CopyIntoBuffer(buffer, ref currentOffset, startTag, true);

                            startTag = [];
                        }

                        if (flags.ScalingTag == TagFlag.Eval && TryParseScalingValue(itemText.Trim('~'), out double scaleValue))
                            GetScalingLocaleText(buffer, ref currentOffset, scaleValue);
                        else if (flags.ScalingTag == TagFlag.Include)
                            CopyIntoBuffer(buffer, ref currentOffset, itemText, false);

                        break;
                    }

                case TextType.SelfCloseTag:
                    {
                        if (flags.ColorTags == TagFlag.Include)
                        {
                            CopyIntoBuffer(buffer, ref currentOffset, itemText, false);
                        }
                    }

                    break;

                default:
                    if (flags.ColorTags == TagFlag.Include && !startTag.IsEmpty)
                    {
                        if (!FontValueCopiedInBuffer(buffer, startTag, ref currentOffset))
                            CopyIntoBuffer(buffer, ref currentOffset, startTag, true);

                        startTag = [];
                        CopyIntoBuffer(buffer, ref currentOffset, itemText, false);
                    }
                    else if (item.Type == TextType.ErrorTag)
                    {
                        if (flags.ErrorTag == TagFlag.Include)
                            CopyIntoBuffer(buffer, ref currentOffset, itemText, false);
                    }
                    else
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
            // Fast path: skip to next special character
            int nextSpecialIndex = gameString[_index..].IndexOfAny(_specialChars);
            if (nextSpecialIndex > 0)
            {
                _index += nextSpecialIndex;
            }
            else if (nextSpecialIndex < 0)
            {
                _index = gameString.Length;
                break;
            }

            if (gameString[_index] == '<' && _index + 1 < gameString.Length && gameString[_index + 1] != ' ')
            {
                int stackCountBeforeText = _textStack.Count;

#if DEBUG
                PushNormalText(gameString);
#else
                PushNormalText();
#endif

                bool pushedText = _textStack.Count > stackCountBeforeText;

                if (TryParseTag(gameString, out Range? tag, out bool isStartTag))
                {
                    if (IsNewLineTag(gameString, tag.Value))
                    {
                        // nested
                        if (startTag.HasValue)
                        {
                            if (TryGetEndTag(gameString, startTag.Value, out Range? endTag))
                                _textStack.Add(new TextRange(endTag.Value, TextType.EndTag));
                            else
                                _textStack.Add(new TextRange(startTag.Value, TextType.MissingEndTag));

                            _textStack.Add(new TextRange(tag.Value, TextType.Newline));
                            _textStack.Add(new TextRange(startTag.Value, TextType.StartTag));
                        }
                        else
                        {
                            _textStack.Add(new TextRange(tag.Value, TextType.Newline));
                        }
                    }
                    else if (IsSpaceTag(gameString, tag.Value))
                    {
                        _textStack.Add(new TextRange(tag.Value, TextType.SpaceTag));
                    }
                    else if (isStartTag)
                    {
                        PushFontVarFromTag(gameString, tag.Value);

                        // nested
                        if (startTag.HasValue)
                        {
                            if (TryGetEndTag(gameString, startTag.Value, out Range? endTag))
                                _textStack.Add(new TextRange(endTag.Value, TextType.EndTag));
                            else
                                _textStack.Add(new TextRange(startTag.Value, TextType.MissingEndTag));
                        }

                        _textStack.Add(new TextRange(tag.Value, TextType.StartTag));

                        ConstructTextStack(gameString, tag);

                        // nested
                        if (startTag.HasValue)
                            _textStack.Add(new TextRange(startTag.Value, TextType.StartTag));
                    }
                    else if (startTag is not null)
                    {
                        if (IsTagsMatch(gameString, startTag.Value, tag.Value))
                        {
                            _textStack.Add(new TextRange(tag.Value, TextType.EndTag));

                            return;
                        }
                    }
                    else if (IsSelfCloseTag(gameString, tag.Value))
                    {
                        _textStack.Add(new TextRange(tag.Value, TextType.SelfCloseTag));
                    }
                }
                else
                {
                    if (pushedText)
                        _textStack.RemoveAt(_textStack.Count - 1);
#if DEBUG
                    PushNormalText(gameString);
#else
                    PushNormalText();
#endif
                }

                _startingIndex = _index;
            }
            else if (gameString[_index] == '~' && _index + 1 < gameString.Length && gameString[_index + 1] == '~')
            {
                int stackCountBeforeText = _textStack.Count;

#if DEBUG
                PushNormalText(gameString);
#else
                PushNormalText();
#endif
                if (TryParseScalingTag(gameString, out Range? tag))
                {
                    _textStack.Add(new TextRange(tag.Value, TextType.ScalingTag));
                    _startingIndex = _index;
                }
                else if (_textStack.Count > stackCountBeforeText)
                {
                    // not a scaling tag, so take back the text and let it carry on past the ~~
                    _textStack.RemoveAt(_textStack.Count - 1);
                }
            }
            else if (gameString[_index] == '#' && _index + 1 < gameString.Length && gameString[_index + 1] == '#')
            {
                int stackCountBeforeText = _textStack.Count;

#if DEBUG
                PushNormalText(gameString);
#else
                PushNormalText();
#endif
                if (TryParseErrorTag(gameString, out Range? tag))
                {
                    _textStack.Add(new TextRange(tag.Value, TextType.ErrorTag));
                    _startingIndex = _index;
                }
                else if (_textStack.Count > stackCountBeforeText)
                {
                    // not an error tag, so take back the text and let it carry on past the ##
                    _textStack.RemoveAt(_textStack.Count - 1);
                }
            }
            else if (gameString[_index] == '%' && _index == _startingIndex && _textStack.Count > 0 && _textStack[^1].Type == TextType.ScalingTag)
            {
                // only when the percent is right after the scaling tag, otherwise the text in between would get moved too
                // increment and push the percent text into the stack
                _index++;
#if DEBUG
                PushNormalText(gameString);
#else
                PushNormalText();
#endif

                // swap the scaling and percent text
                (_textStack[^1], _textStack[^2]) = (_textStack[^2], _textStack[^1]);

                _startingIndex = _index;
            }
            else
            {
                _index++;
            }
        }

        if (_index <= gameString.Length)
        {
            // reached the end of the string without finding the matching end tag (that case returns early)
            bool missingEndTag = startTag.HasValue;

#if DEBUG
            PushNormalText(gameString);
#else
            PushNormalText();
#endif
            if (missingEndTag)
                _textStack.Add(new TextRange(startTag!.Value, TextType.MissingEndTag));
        }
    }

#if DEBUG
    private void PushNormalText(ReadOnlySpan<char> gameString)
#else
    private void PushNormalText()
#endif
    {
        int normalTextLength = _index - _startingIndex;
        if (normalTextLength > 0)
        {
#if DEBUG
            ReadOnlySpan<char> temp = gameString.Slice(_startingIndex, normalTextLength);
#endif
            _textStack.Add(new TextRange(_startingIndex, _index, TextType.Text));
        }
    }

    // try to parse out a tag
    private bool TryParseTag(ReadOnlySpan<char> gameString, [NotNullWhen(true)] out Range? tag, out bool isStartTag)
    {
        tag = null;
        isStartTag = false;

        ReadOnlySpan<char> currentTextSpan = gameString[_index..];
        int lengthOffset = gameString.Length - currentTextSpan.Length;

        int startTagIndex = 0; // index of <
        int endTagIndex = -1;

        // the next < or > after the starting <
        int nextBracketIndex = currentTextSpan[1..].IndexOfAny('<', '>') + 1;
        if (nextBracketIndex > 0)
        {
            if (currentTextSpan[nextBracketIndex] == '>')
                endTagIndex = nextBracketIndex;
            else
                startTagIndex = nextBracketIndex;
        }

        if (endTagIndex > 0)
        {
            ReadOnlySpan<char> tagSpan = currentTextSpan[startTagIndex..(endTagIndex + 1)];

            // check if its a start tag
            if (tagSpan[1] != '/' && tagSpan[^2] != '/')
                isStartTag = true;
            else
                isStartTag = false;

            tag = new Range(startTagIndex + lengthOffset, endTagIndex + lengthOffset + 1);

            _index += endTagIndex - startTagIndex + 1;

            return true;
        }

        if (startTagIndex == 0)
            _index++;
        else
            _index += startTagIndex;

        return false;
    }

    private bool TryParseScalingTag(ReadOnlySpan<char> gameString, [NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        ReadOnlySpan<char> currentTextSpan = gameString[_index..];
        int lengthOffset = gameString.Length - currentTextSpan.Length;

        int startScaleIndex = currentTextSpan.IndexOf("~~") + 1; // the second char of the ~~ (first batch)
        int endScaleIndex = currentTextSpan[(startScaleIndex + 1)..].IndexOf("~~"); // first char of the ~~ (second batch)
        if (endScaleIndex >= 0)
            endScaleIndex += startScaleIndex + 1;

        if (startScaleIndex > 0 && endScaleIndex > 0)
        {
            ReadOnlySpan<char> value = currentTextSpan[(startScaleIndex + 1)..endScaleIndex];

            if (IsDigits(value, _decimalSeparator))
            {
                tag = new Range(startScaleIndex - 1 + lengthOffset, endScaleIndex + 1 + lengthOffset + 1);

                _index += endScaleIndex - startScaleIndex + 3;

                return true;
            }
        }

        _index += 2;

        return false;
    }

    private bool TryParseErrorTag(ReadOnlySpan<char> gameString, [NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        ReadOnlySpan<char> currentTextSpan = gameString[_index..];
        int lengthOffset = gameString.Length - currentTextSpan.Length;

        if (currentTextSpan.StartsWith(GameStringText.ErrorTag, StringComparison.Ordinal))
        {
            tag = new Range(lengthOffset, GameStringText.ErrorTag.Length + lengthOffset);

            _index += GameStringText.ErrorTag.Length;

            return true;
        }

        _index += 2;

        return false;
    }

    // find the index of the end tag
    private bool TryGetEndTag(ReadOnlySpan<char> gameString, Range startTag, [NotNullWhen(true)] out Range? endTag)
    {
        endTag = null;

        ReadOnlySpan<char> currentTextSpan = gameString[_index..];
        ReadOnlySpan<char> startTagSpan = gameString[startTag];

        // find the tag type
        ReadOnlySpan<char> tagTypeSpan = GetEndTagCharType(startTagSpan);

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

        return false;
    }

    private void PushFontVarFromTag(ReadOnlySpan<char> gameString, Range tag)
    {
        if (!ExtractFontVars)
            return;

        // <c val=\"#TooltipNumbers\">
        // <s val=\"StandardTooltipHeader\">
        ReadOnlySpan<char> fontSpan = gameString[tag];

        if (fontSpan.StartsWith("<c", StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> fontTagVal = GetFontTagVal(fontSpan);

            if (!fontTagVal.IsEmpty)
                AddFontVar(_styleConstantTagVariables, fontTagVal);
        }
        else if (fontSpan.StartsWith("<s", StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> fontTagVal = GetFontTagVal(fontSpan);

            if (!fontTagVal.IsEmpty)
                AddFontVar(_styleTagVariables, fontTagVal);
        }
        else
        {
            return;
        }
    }

    private bool FontValueCopiedInBuffer(Span<char> buffer, ReadOnlySpan<char> startTag, ref int currentOffset)
    {
        bool isStyleTag = startTag.StartsWith("<s", StringComparison.OrdinalIgnoreCase);
        bool isConstantTag = startTag.StartsWith("<c", StringComparison.OrdinalIgnoreCase);

        if (!isStyleTag && !isConstantTag)
            return false;

        // only the tag type that has hlt-name removal turned on is affected, the other tag type still gets its value replacements
        bool removeHltName = (isStyleTag && RemoveHltNameFromStyleTags) || (isConstantTag && RemoveHltNameFromConstantTags);

        if (removeHltName)
        {
            // removing the hlt-name never makes the tag longer, so it fits in a buffer the length of the tag
            char[]? rentedBuffer = null;
            Span<char> tagBuffer = startTag.Length <= 256 ? stackalloc char[256] : (rentedBuffer = ArrayPool<char>.Shared.Rent(startTag.Length));

            int tagLength = 0;
            bool isHltNameRemoved = HltNameRemovedInBuffer(tagBuffer, startTag, isStyleTag, ref tagLength);

            if (isHltNameRemoved)
            {
                // the hlt-name removal goes first, then the replacement is for the val that is left
                ReadOnlySpan<char> tagWithoutHltName = tagBuffer[..tagLength];

                if (!CopiedInFontTagVal(buffer, tagWithoutHltName, ref currentOffset))
                    CopyIntoBuffer(buffer, ref currentOffset, tagWithoutHltName, false);
            }

            if (rentedBuffer is not null)
                ArrayPool<char>.Shared.Return(rentedBuffer);

            if (isHltNameRemoved)
                return true;
        }

        return CopiedInFontTagVal(buffer, startTag, ref currentOffset);
    }

    // the val value that a replacement is looked up by, which is the hlt-name value when the hlt-name removal undoes the replacement
    private ReadOnlySpan<char> GetFontTagValForReplacement(ReadOnlySpan<char> startTag, bool isStyleTag)
    {
        bool undoHltName = isStyleTag
            ? RemoveHltNameFromStyleTags && UndoHltNameReplacementForStyleTags
            : RemoveHltNameFromConstantTags && UndoHltNameReplacementForConstantTags;

        if (undoHltName)
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
    private bool HltNameRemovedInBuffer(Span<char> buffer, ReadOnlySpan<char> startTag, bool isStyleTag, ref int currentOffset)
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

        bool undoReplacement = isStyleTag ? UndoHltNameReplacementForStyleTags : UndoHltNameReplacementForConstantTags;
        int tagStartOffset = currentOffset;

        if (undoReplacement)
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
    private bool CopiedInFontTagVal(Span<char> buffer, ReadOnlySpan<char> startTag, ref int currentOffset)
    {
#if NET9_0_OR_GREATER
        if (_valueByStyleVarAltLookup is null && _valueByStyleConstantVarAltLookup is null)
#else
        if (_valueByStyleVar is null && _valueByStyleConstantVar is null)
#endif
            return false;

        // "<s val=\"StandardTooltipHeader\">"
        ReadOnlySpan<char> fontTagVal = GetFontTagVal(startTag);

        if (fontTagVal.IsEmpty is false &&
#if NET9_0_OR_GREATER
            ((_valueByStyleConstantVarAltLookup is not null && startTag.StartsWith("<c", StringComparison.OrdinalIgnoreCase) && _valueByStyleConstantVarAltLookup.Value.TryGetValue(fontTagVal, out var newValue)) ||
            (_valueByStyleVarAltLookup is not null && startTag.StartsWith("<s", StringComparison.OrdinalIgnoreCase) && _valueByStyleVarAltLookup.Value.TryGetValue(fontTagVal, out newValue))))
#else
            ((_valueByStyleConstantVar is not null && startTag.StartsWith("<c", StringComparison.OrdinalIgnoreCase) && _valueByStyleConstantVar.TryGetValue(fontTagVal.ToString(), out var newValue)) ||
            (_valueByStyleVar is not null && startTag.StartsWith("<s", StringComparison.OrdinalIgnoreCase) && _valueByStyleVar.TryGetValue(fontTagVal.ToString(), out newValue))))
#endif
        {
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
                _hltNameAttribute.AsSpan().CopyTo(buffer[currentOffset..]);
                currentOffset += _hltNameAttribute.Length;
                buffer[currentOffset++] = '"';
                fontTagVal.CopyTo(buffer[currentOffset..]);
                currentOffset += fontTagVal.Length;
            }

            // \"> or \" otherAtt=\"somevalue\">
            CopyIntoBuffer(buffer, ref currentOffset, startTag[indexOfEndQuote..], false);

            // cleaned as a whole, the same as a tag with no replacement
            currentOffset = tagStartOffset + CleanUpTag(buffer[tagStartOffset..currentOffset]);

            return true;
        }

        return false;
    }

    private int GetSizeOfBuffer(ReadOnlySpan<char> gameString, GameStringFlags flags)
    {
        int sum = 0;

        foreach (TextRange current in CollectionsMarshal.AsSpan(_textStack))
        {
            switch (current.Type)
            {
                case TextType.MissingEndTag:
                    {
                        // find the size of the tag
                        ReadOnlySpan<char> currentSpan = current.GetText(gameString);
                        ReadOnlySpan<char> tagTypeSpan = GetEndTagCharType(currentSpan);

                        sum += tagTypeSpan.Length + 3; // 3 for </>
                        break;
                    }

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
                    {
                        if (flags.ColorTags == TagFlag.Include)
                        {
                            ReadOnlySpan<char> currentSpan = current.GetText(gameString);
#if NET9_0_OR_GREATER
                            if (currentSpan.StartsWith("<c", StringComparison.OrdinalIgnoreCase) && _valueByStyleConstantVarAltLookup is not null)
#else
                            if (currentSpan.StartsWith("<c", StringComparison.OrdinalIgnoreCase) && _valueByStyleConstantVar is not null)
#endif
                            {
                                ReadOnlySpan<char> tagVarVal = GetFontTagValForReplacement(currentSpan, isStyleTag: false); // #TooltipNumbers
#if NET9_0_OR_GREATER
                                if (_valueByStyleConstantVarAltLookup.Value.TryGetValue(tagVarVal, out var newValue))
#else
                                if (_valueByStyleConstantVar.TryGetValue(tagVarVal.ToString(), out var newValue))
#endif
                                {
                                    int tagVarValLength = tagVarVal.Length;
                                    sum += currentSpan.Length - tagVarValLength + Math.Max(tagVarValLength, newValue.Value.Length);

                                    if (newValue.Preserve)
                                        sum += 3 + _hltNameAttribute.Length + tagVarValLength; // hlt-name="#TooltipNumbers" +3 for space and quotes

                                    break;
                                }
                            }
#if NET9_0_OR_GREATER
                            else if (currentSpan.StartsWith("<s", StringComparison.OrdinalIgnoreCase) && _valueByStyleVarAltLookup is not null)
#else
                            else if (currentSpan.StartsWith("<s", StringComparison.OrdinalIgnoreCase) && _valueByStyleVar is not null)
#endif
                            {
                                ReadOnlySpan<char> tagVarVal = GetFontTagValForReplacement(currentSpan, isStyleTag: true); // "StandardTooltipHeader"
#if NET9_0_OR_GREATER
                                if (_valueByStyleVarAltLookup.Value.TryGetValue(tagVarVal, out var newValue))
#else
                                if (_valueByStyleVar.TryGetValue(tagVarVal.ToString(), out var newValue))
#endif
                                {
                                    int tagVarValLength = tagVarVal.Length;
                                    sum += currentSpan.Length - tagVarValLength + Math.Max(tagVarValLength, newValue.Value.Length);

                                    if (newValue.Preserve)
                                        sum += 3 + _hltNameAttribute.Length + tagVarValLength; // hlt-name="StandardTooltipHeader" +3 for space and quotes

                                    break;
                                }
                            }
                        }

                        goto case TextType.EndTag;
                    }

                case TextType.EndTag:
                    if (flags.ColorTags == TagFlag.Include)
                        goto default;
                    break;
                case TextType.ScalingTag:
                    if (flags.ScalingTag == TagFlag.Include)
                        goto default;
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

    private void GetScalingLocaleText(Span<char> buffer, ref int offset, double value)
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
