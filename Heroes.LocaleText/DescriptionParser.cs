using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Heroes.LocaleText;

/// <summary>
/// Used for parsing through a gamestring that has been already been xml parsed out and computed for values.
/// </summary>
internal class DescriptionParser
{
    private readonly string _description;
    private readonly StormLocale _gameStringLocale;
    private readonly List<TextRange> _textStack = [];

    private bool _isContructed = false;
    private int _startingIndex = 0;
    private int _index = 0;

    private DescriptionParser(string description, StormLocale gameStringLocale)
    {
        _description = description;
        _gameStringLocale = gameStringLocale;
    }

    ///// <summary>
    ///// Takes a gamestring and removes unmatched tags and modifies nested tags into unnested tags.
    ///// </summary>
    ///// <param name="gameString">The gamestring text.</param>
    ///// <returns>A modified gamestring.</returns>
    public static DescriptionParser Validate(string gameString, StormLocale gameStringLocale = StormLocale.ENUS)
    {
        return new DescriptionParser(gameString, gameStringLocale);
    }

    public string GetRawDescription()
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
    ///// <summary>
    ///// Returns a plain text string without any tags.
    ///// </summary>
    ///// <param name="gameString">The gamestring text.</param>
    ///// <param name="includeNewLineTags">If true, includes the newline tags.</param>
    ///// <param name="includeScaling">If true, includes the scaling info.</param>
    ///// <param name="stormLocale">Localization for the gamestring.</param>
    ///// <returns>A modified gamestring.</returns>
    //public static string GetPlainText(ReadOnlySpan<char> gameString, bool includeNewLineTags, bool includeScaling, StormLocale stormLocale = StormLocale.ENUS)
    //{
    //    return new DescriptionParserCopy(stormLocale).ParseToPlainText(gameString, includeNewLineTags, includeScaling);
    //}

    ///// <summary>
    ///// Returns the string with all tags.
    ///// </summary>
    ///// <param name="gameString">The gamestring text.</param>
    ///// <param name="includeScaling">If true, includes the scaling info.</param>
    ///// <param name="stormLocale">Localization for the gamestring.</param>
    ///// <returns>A modified gamestring.</returns>
    //public static string GetColoredText(ReadOnlySpan<char> gameString, bool includeScaling, StormLocale stormLocale = StormLocale.ENUS)
    //{
    //    return new DescriptionParserCopy(stormLocale).ParseToColoredText(gameString, includeScaling);
    //}

    private static int CopyIntoBuffer(Span<char> buffer, int offset, ReadOnlySpan<char> item, bool cleanTheTag)
    {
        item.CopyTo(buffer[offset..]);

        if (cleanTheTag)
        {
            offset += CleanUpTag(buffer.Slice(offset, item.Length));
            //Span<char> tempTagBuffer = item.Length < 1024 ? stackalloc char[item.Length] : new char[item.Length];
            //item.CopyTo(tempTagBuffer);
            //CleanUpTag(ref tempTagBuffer);

            //tempTagBuffer.CopyTo(buffer[offset..]);
            //offset += item.Length;
        }
        else
        {
            //item.CopyTo(buffer[offset..]);
            offset += item.Length;
        }

        return offset;
    }

    // replaces double space or more into single space, and lowercases the tag type
    private static int CleanUpTag(Span<char> text)
    {
        int position;
        int i;
        for (i = 0, position = 0; i < text.Length && position < text.Length; i++, position++)
        {
            while (text[position] == ' ' && i < text.Length && text[position + 1] == ' ')
            {
                position++;
            }

            text[i] = text[position];
        }

        text = text[..(position - (position - i))];

        Span<char> tagTypeSpan;
        int spaceIndex = text.IndexOf(' ');

        if (spaceIndex > -1)
            tagTypeSpan = text[..spaceIndex].TrimStart('<');
        else
            tagTypeSpan = text.Trim("</>");
        for (int j = 0; j < tagTypeSpan.Length; j++)
        {
            tagTypeSpan[j] = char.ToLowerInvariant(tagTypeSpan[j]);
        }

        return text.Length;
    }

    // checks if the end tag matches the start tag
    private static bool IsTagsMatch(ReadOnlySpan<char> text, Range startTag, Range endTag)
    {
        ReadOnlySpan<char> startSpan = text[startTag];
        ReadOnlySpan<char> endSpan = text[endTag].Trim("</> ");
        ReadOnlySpan<char> firstPart;

        int spaceIndex = startSpan.IndexOf(' ');

        if (spaceIndex > -1)
            firstPart = startSpan[..spaceIndex].TrimStart('<');
        else
            firstPart = startSpan.TrimStart('<');

        return firstPart.Equals(endSpan, StringComparison.OrdinalIgnoreCase);
    }

    private static ReadOnlySpan<char> GetEndTagCharType(ReadOnlySpan<char> startTagText)
    {
        ReadOnlySpan<char> tagTypeSpan;

        int spaceIndex = startTagText.IndexOf(' ');
        if (spaceIndex > -1)
            tagTypeSpan = startTagText[..spaceIndex].TrimStart('<');
        else
            tagTypeSpan = startTagText.Trim("<>");

        return tagTypeSpan;
    }

    private static bool IsNewLineTag(ReadOnlySpan<char> text, Range tag) => IsNewLineTag(text[tag]);

    private static bool IsNewLineTag(ReadOnlySpan<char> text) => text.Equals("<n/>", StringComparison.OrdinalIgnoreCase) || text.Equals("</n>", StringComparison.OrdinalIgnoreCase);

    private static bool IsSpaceTag(ReadOnlySpan<char> text, Range tag) => IsSpaceTag(text[tag]);

    private static bool IsSpaceTag(ReadOnlySpan<char> text) => text.Equals("<sp/>", StringComparison.OrdinalIgnoreCase);

    private static bool IsSelfCloseTag(ReadOnlySpan<char> text, Range tag)
    {
        ReadOnlySpan<char> tagSpan = text[tag];

        return !tagSpan.Equals("<li/>", StringComparison.OrdinalIgnoreCase) && tagSpan.Length > 3 && tagSpan.EndsWith("/>", StringComparison.OrdinalIgnoreCase);
    }

    private string Parse(ReadOnlySpan<char> gameString)
    {
        if (!_isContructed)
        {
            ConstructTextStack(gameString);
            _isContructed = true;
        }

        return BuildDescription(gameString, new DescriptionFlags()
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
        if (!_isContructed)
        {
            ConstructTextStack(gameString);
            _isContructed = true;
        }

        return BuildDescription(gameString, new DescriptionFlags()
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
        if (!_isContructed)
        {
            ConstructTextStack(gameString);
            _isContructed = true;
        }

        return BuildDescription(gameString, new DescriptionFlags()
        {
            ColorTags = TagFlag.Include,
            ScalingTag = includeScaling ? TagFlag.Eval : TagFlag.None,
            NewLineTag = TagFlag.Include,
            ErrorTag = TagFlag.None,
            SpaceTag = TagFlag.Eval,
        });
    }

    private string BuildDescription(ReadOnlySpan<char> gameString, DescriptionFlags flags)
    {
        if (_textStack.Count < 1)
            return string.Empty;

        int totalSize = GetSizeOfBuffer(gameString, flags);

        Span<char> buffer = totalSize < 1024 ? stackalloc char[totalSize] : new char[totalSize];

        ReadOnlySpan<char> startTag = [];
        int currentOffset = 0;

        // loop through and build string
        for (int i = 0; i < _textStack.Count; i++)
        {
            TextRange item = _textStack[i];
            ReadOnlySpan<char> itemText = gameString[item.Range];

            switch (item.Type)
            {
                case TextType.Newline:
                    if (flags.NewLineTag == TagFlag.Include)
                        currentOffset = CopyIntoBuffer(buffer, currentOffset, "<n/>", false);
                    else if (flags.NewLineTag == TagFlag.Eval)
                        currentOffset = CopyIntoBuffer(buffer, currentOffset, " ", false);
                    break;
                case TextType.SpaceTag:
                    if (flags.SpaceTag == TagFlag.Include)
                        currentOffset = CopyIntoBuffer(buffer, currentOffset, itemText, false);
                    else if (flags.SpaceTag == TagFlag.Eval)
                        currentOffset = CopyIntoBuffer(buffer, currentOffset, " ", false);
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
                                currentOffset = CopyIntoBuffer(buffer, currentOffset, itemText, true);
                            else
                                currentOffset = CopyIntoBuffer(buffer, currentOffset, $"</{GetEndTagCharType(itemText)}>", false);
                        }
                        else // dont save, empty tag
                        {
                            startTag = [];
                        }
                    }

                    break;
                case TextType.ScalingTag:
                    {
                        if (flags.ScalingTag == TagFlag.Eval && double.TryParse(itemText.Trim('~'), CultureInfo.InvariantCulture, out double scaleValue))
                        {
                            currentOffset = CopyIntoBuffer(buffer, currentOffset, GetPerLevelLocale(scaleValue), false);
                        }
                        else if (flags.ScalingTag == TagFlag.Include)
                        {
                            currentOffset = CopyIntoBuffer(buffer, currentOffset, itemText, false);
                        }

                        break;
                    }

                default:
                    if (flags.ColorTags == TagFlag.Include && !startTag.IsEmpty)
                    {
                        currentOffset = CopyIntoBuffer(buffer, currentOffset, startTag, true);
                        startTag = [];
                        currentOffset = CopyIntoBuffer(buffer, currentOffset, itemText, false);
                    }
                    else if (item.Type == TextType.ErrorTag)
                    {
                        if (flags.ErrorTag == TagFlag.Include)
                            currentOffset = CopyIntoBuffer(buffer, currentOffset, itemText, false);
                    }
                    else
                    {
                        currentOffset = CopyIntoBuffer(buffer, currentOffset, itemText, false);
                    }

                    break;
            }
        }

        // since we filled the buffer from the end, it could have null chars at the beginning
        return buffer.TrimEnd('\0').ToString();
    }

    private void ConstructTextStack(ReadOnlySpan<char> gameString, Range? startTag = null)
    {
        _startingIndex = _index;

        while (_index < gameString.Length)
        {
            if (gameString[_index] == '<' && _index + 1 < gameString.Length && gameString[_index + 1] != ' ')
            {
#if DEBUG
                PushNormalText(gameString);
#else
                PushNormalText();
#endif
                if (TryParseTag(gameString, out Range? tag, out bool isStartTag))
                {
                    if (isStartTag)
                    {
                        // nested
                        if (startTag.HasValue)
                        {
                            if (TryGetEndTag(gameString, startTag.Value, out Range? endTag))
                                _textStack.Add(new TextRange(endTag.Value, TextType.EndTag));
                            else
                                _textStack.Add(new TextRange(startTag!.Value, TextType.MissingEndTag));
                        }

                        _textStack.Add(new TextRange(tag!.Value, TextType.StartTag));

                        ConstructTextStack(gameString, tag);

                        // nested
                        if (startTag.HasValue)
                            _textStack.Add(new TextRange(startTag!.Value, TextType.StartTag));
                    }
                    else if (IsNewLineTag(gameString, tag.Value))
                    {
                        // nested
                        if (startTag.HasValue)
                        {
                            if (TryGetEndTag(gameString, startTag.Value, out Range? endTag))
                                _textStack.Add(new TextRange(endTag.Value, TextType.EndTag));

                            _textStack.Add(new TextRange(tag.Value, TextType.Newline));
                            _textStack.Add(new TextRange(startTag.Value, TextType.StartTag));
                        }
                        else
                        {
                            _textStack.Add(new TextRange(tag.Value, TextType.Newline));
                        }
                    }
                    else if (startTag is not null)
                    {
                        if (IsTagsMatch(gameString, startTag.Value, tag.Value))
                        {
                            _textStack.Add(new TextRange(tag.Value, TextType.EndTag));

                            return;
                        }
                    }
                    else if (IsSpaceTag(gameString, tag.Value))
                    {
                        _textStack.Add(new TextRange(tag.Value, TextType.SpaceTag));
                    }
                    else if (IsSelfCloseTag(gameString, tag.Value))
                    {
                        _textStack.Add(new TextRange(tag.Value, TextType.SelfCloseTag));
                    }
                }
                else
                {
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
#if DEBUG
                PushNormalText(gameString);
#else
                PushNormalText();
#endif
                if (TryParseScalingTag(gameString, out Range? tag))
                {
                    _textStack.Add(new TextRange(tag.Value, TextType.ScalingTag));
                }
                else
                {
                    _textStack.RemoveAt(_textStack.Count - 1);
#if DEBUG
                    PushNormalText(gameString);
#else
                    PushNormalText();
#endif
                }

                _startingIndex = _index;
            }
            else if (gameString[_index] == '#' && _index + 1 < gameString.Length && gameString[_index + 1] == '#')
            {
#if DEBUG
                PushNormalText(gameString);
#else
                PushNormalText();
#endif
                if (TryParseErrorTag(gameString, out Range? tag))
                {
                    _textStack.Add(new TextRange(tag.Value, TextType.ErrorTag));
                }

                _startingIndex = _index;
            }
            else
            {
                _index++;
            }
        }

        if (_index <= gameString.Length)
        {
            bool missingEndTag = _textStack.Count > 0 && startTag.HasValue && _textStack[^1].Range.Equals(startTag);

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
    {
        int normalTextLength = _index - _startingIndex;
        if (normalTextLength > 0)
        {
            ReadOnlySpan<char> temp = gameString.Slice(_startingIndex, normalTextLength);

            _textStack.Add(new TextRange(new Range(_startingIndex, _index), TextType.Text));
        }
    }
#else
    private void PushNormalText()
    {
        int normalTextLength = _index - _startingIndex;
        if (normalTextLength > 0)
        {
            _textStack.Add(new TextRange(new Range(_startingIndex, _index), TextType.Text));
        }
    }
#endif

    // try to parse out a tag
    private bool TryParseTag(ReadOnlySpan<char> gameString, [NotNullWhen(true)] out Range? tag, out bool isStartTag)
    {
        tag = null;
        isStartTag = false;

        ReadOnlySpan<char> currentTextSpan = gameString[_index..];
        int lengthOffset = gameString.Length - currentTextSpan.Length;

        int startTagIndex = 0; // index of <
        int endTagIndex = -1;

        for (int i = 1; i < currentTextSpan.Length; i++)
        {
            if (currentTextSpan[i] == '>')
            {
                endTagIndex = i;
                break;
            }
            else if (currentTextSpan[i] == '<')
            {
                startTagIndex = i;
                break;
            }
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

        _index += startTagIndex;

        return false;
    }

    private bool TryParseScalingTag(ReadOnlySpan<char> gameString, [NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        ReadOnlySpan<char> currentTextSpan = gameString[_index..];
        int lengthOffset = gameString.Length - currentTextSpan.Length;

        int startScaleIndex = currentTextSpan.IndexOf("~~") + 1; // the second char of the ~~ (first batch)
        int endScaleIndex = -1; // first char of the ~~ (second batch)

        for (int i = startScaleIndex; i < currentTextSpan.Length; i++)
        {
            // find next occurrence of ~~
            if (currentTextSpan[i] == '~' && i + 1 < currentTextSpan.Length && currentTextSpan[i + 1] == '~')
            {
                endScaleIndex = i;

                break;
            }
        }

        if (startScaleIndex > 0 && endScaleIndex > 0)
        {
            ReadOnlySpan<char> value = currentTextSpan[(startScaleIndex + 1)..endScaleIndex];

            if (double.TryParse(value, out _))
            {
                tag = new Range(startScaleIndex - 1 + lengthOffset, endScaleIndex + 1 + lengthOffset + 1);

                _index += endScaleIndex - startScaleIndex + 3;

                return true;
            }
        }

        _index += endScaleIndex + 2;

        return false;
    }

    private bool TryParseErrorTag(ReadOnlySpan<char> gameString, [NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        ReadOnlySpan<char> currentTextSpan = gameString[_index..];
        int lengthOffset = gameString.Length - currentTextSpan.Length;

        if (currentTextSpan.StartsWith("##ERROR##", StringComparison.OrdinalIgnoreCase))
        {
            tag = new Range(lengthOffset, 9 + lengthOffset);

            _index += 9;

            return true;
        }

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

        int indexOfEndTag = currentTextSpan.IndexOf($"</{tagTypeSpan}>", StringComparison.OrdinalIgnoreCase);

        if (indexOfEndTag > -1)
        {
            endTag = new Range(_index + indexOfEndTag, _index + indexOfEndTag + tagTypeSpan.Length + 3);
            return true;
        }

        return false;
    }

    private int GetSizeOfBuffer(ReadOnlySpan<char> gameString, DescriptionFlags flags)
    {
        int sum = 0;

        for (int i = _textStack.Count - 1; i >= 0; i--)
        {
            TextRange current = _textStack[i];

            switch (current.Type)
            {
                case TextType.MissingEndTag:
                    {
                        // find the size of the tag
                        ReadOnlySpan<char> currentSpan = gameString[current.Range];
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
                case TextType.EndTag:
                    if (flags.ColorTags == TagFlag.Include)
                        sum += current.Range.End.Value - current.Range.Start.Value;
                    break;
                case TextType.ScalingTag:
                    sum += 21 + 23; // 21 largest min text, 23 for double
                    break;
                default:
                    sum += current.Range.End.Value - current.Range.Start.Value;
                    break;
            }
        }

        return sum;
    }

    private ReadOnlySpan<char> GetPerLevelLocale(double value)
    {
        return _gameStringLocale switch
        {
            StormLocale.ENUS => $"{value.ToString(" (+0.##% per level)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.DEDE => $"{value.ToString(" (+0.##% pro Stufe)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.ESES => $"{value.ToString(" (+0.##% por nivel)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.ESMX => $"{value.ToString(" (+0.##% por nivel)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.FRFR => $"{value.ToString(" (+0.##% par niveau)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.ITIT => $"{value.ToString(" (+0.##% per livello)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.KOKR => $"{value.ToString(" (레벨당 +0.##%)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.PLPL => $"{value.ToString(" (+0.##% na poziom)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.PTBR => $"{value.ToString(" (+0.##% por nível)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.RURU => $"{value.ToString(" (+0.##% за уровень)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.ZHCN => $"{value.ToString(" (每级+0.##%)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
            StormLocale.ZHTW => $"{value.ToString(" (每級+0.##%)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",

            _ => $"{value.ToString(" (+0.##% per level)", StormLocaleData.GetCultureInfo(_gameStringLocale))}",
        };
    }
}
