using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Heroes.LocaleText;

/// <summary>
/// Used for parsing through a gamestring that has been already been xml parsed out and computed for values.
/// </summary>
internal class DescriptionParserOld
{
    private readonly StormLocale _gameStringLocale;
    private readonly Stack<Range> _textStack;
    private readonly HashSet<int> _missingEndTagsByStackCount = [];
    private readonly HashSet<int> _scaleValueByStackCount = [];

    private int _startingIndex;
    private int _index;

    private DescriptionParserOld(StormLocale gameStringLocale = StormLocale.ENUS)
    {
        _gameStringLocale = gameStringLocale;

        _startingIndex = 0;
        _index = 0;
        _textStack = [];
    }

    /// <summary>
    /// Takes a gamestring and removes unmatched tags and modifies nested tags into unnested tags.
    /// </summary>
    /// <param name="gameString">The gamestring text.</param>
    /// <returns>A modified gamestring.</returns>
    public static string Validate(ReadOnlySpan<char> gameString)
    {
        return new DescriptionParserOld().Parse(gameString);
    }

    /// <summary>
    /// Returns a plain text string without any tags.
    /// </summary>
    /// <param name="gameString">The gamestring text.</param>
    /// <param name="includeNewLineTags">If true, includes the newline tags.</param>
    /// <param name="includeScaling">If true, includes the scaling info.</param>
    /// <param name="stormLocale">Localization for the gamestring.</param>
    /// <returns>A modified gamestring.</returns>
    public static string GetPlainText(ReadOnlySpan<char> gameString, bool includeNewLineTags, bool includeScaling, StormLocale stormLocale = StormLocale.ENUS)
    {
        return new DescriptionParserOld(stormLocale).ParseToPlainText(gameString, includeNewLineTags, includeScaling);
    }

    /// <summary>
    /// Returns the string with all tags.
    /// </summary>
    /// <param name="gameString">The gamestring text.</param>
    /// <param name="includeScaling">If true, includes the scaling info.</param>
    /// <param name="stormLocale">Localization for the gamestring.</param>
    /// <returns>A modified gamestring.</returns>
    public static string GetColoredText(ReadOnlySpan<char> gameString, bool includeScaling, StormLocale stormLocale = StormLocale.ENUS)
    {
        return new DescriptionParserOld(stormLocale).ParseToColoredText(gameString, includeScaling);
    }

    private static int CopyIntoBuffer(Span<char> buffer, int offset, ReadOnlySpan<char> item, bool cleanTheTag)
    {
        if (cleanTheTag)
        {
            Span<char> tempTagBuffer = item.Length < 1024 ? stackalloc char[item.Length] : new char[item.Length];
            item.CopyTo(tempTagBuffer);
            CleanUpTag(ref tempTagBuffer);

            tempTagBuffer.CopyTo(buffer.Slice(offset - tempTagBuffer.Length, tempTagBuffer.Length));
        }
        else
        {
            item.CopyTo(buffer.Slice(offset - item.Length, item.Length));
        }

        offset -= item.Length;

        return offset;
    }

    // replaces double space or more into single space, and lowercases the tag type
    private static void CleanUpTag(ref Span<char> text)
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

        Span<char> tagTypeSpan = text;
        int spaceIndex = text.IndexOf(' ');

        if (spaceIndex > -1)
            tagTypeSpan = text[..spaceIndex];

        for (int j = 0; j < tagTypeSpan.Length; j++)
        {
            tagTypeSpan[j] = char.ToLowerInvariant(tagTypeSpan[j]);
        }
    }

    // checks if the end tag matches the start tag
    private static bool IsTagsMatch(ReadOnlySpan<char> gameString, Range startTag, Range endTag)
    {
        ReadOnlySpan<char> startSpan = gameString[startTag];
        ReadOnlySpan<char> endSpan = gameString[endTag].Trim("</> ");
        ReadOnlySpan<char> firstPart;

        int spaceIndex = startSpan.IndexOf(' ');

        if (spaceIndex > -1)
            firstPart = startSpan[..spaceIndex].TrimStart('<');
        else
            firstPart = startSpan.TrimStart('<');

        return firstPart.Equals(endSpan, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNewLineTag(ReadOnlySpan<char> gameString, Range tag) => IsNewLineTag(gameString[tag]);

    private static bool IsNewLineTag(ReadOnlySpan<char> gameString) => gameString.Equals("<n/>", StringComparison.OrdinalIgnoreCase) || gameString.Equals("</n>", StringComparison.OrdinalIgnoreCase);

    private static bool IsSpaceTag(ReadOnlySpan<char> gameString, Range tag) => IsSpaceTag(gameString[tag]);

    private static bool IsSpaceTag(ReadOnlySpan<char> gameString) => gameString.Equals("<sp/>", StringComparison.OrdinalIgnoreCase);

    private static bool IsSelfCloseTag(ReadOnlySpan<char> gameString, Range tag)
    {
        ReadOnlySpan<char> tagSpan = gameString[tag];

        return !tagSpan.Equals("<li/>", StringComparison.OrdinalIgnoreCase) && tagSpan.Length > 3 && tagSpan.EndsWith("/>", StringComparison.OrdinalIgnoreCase);
    }

    private string Parse(ReadOnlySpan<char> gameString)
    {
        // build up the stack
        NestedTagCleanUp(gameString);

        // then build the string
        return BuildDescription(gameString, true, false);
    }

    private string ParseToPlainText(ReadOnlySpan<char> gameString, bool includeNewlineTags, bool includeScaling)
    {
        PlainTextTagCleanup(gameString, includeScaling);

        return BuildDescription(gameString, includeNewlineTags, true);
    }

    private string ParseToColoredText(ReadOnlySpan<char> gameString, bool includeScaling)
    {
        ColoredTextTagCleanup(gameString, includeScaling);

        return BuildDescription(gameString, true, false);
    }

    private string BuildDescription(ReadOnlySpan<char> gameString, bool includeNewlineTags, bool eval)
    {
        if (_textStack.Count < 1)
            return string.Empty;

        ReadOnlySpan<char> endTag = [];
        ReadOnlySpan<char> firstItem = gameString[_textStack.Peek()];

        // remove unmatched start tag
        if (!_missingEndTagsByStackCount.Contains(_textStack.Count) && firstItem[0] == '<' && firstItem[^1] == '>' &&
            !firstItem.EndsWith("/>") && !firstItem.StartsWith("</"))
        {
            _textStack.Pop();
        }

        int totalSize = GetSizeOfBuffer(gameString);

        Span<char> buffer = totalSize < 1024 ? stackalloc char[totalSize] : new char[totalSize];
        int currentCopyIndex = buffer.Length;

        // loop through stack and build string
        // the buffer is being filled from the end
        while (_textStack.Count > 0)
        {
            ReadOnlySpan<char> item = gameString[_textStack.Pop()];

            if (_missingEndTagsByStackCount.Contains(_textStack.Count + 1))
            {
                ReadOnlySpan<char> tagTypeSpan;

                int spaceIndex = item.IndexOf(' ');
                if (spaceIndex > -1)
                    tagTypeSpan = item[..spaceIndex].TrimStart('<');
                else
                    tagTypeSpan = item.Trim("<>");

                endTag = $"</{tagTypeSpan}>";

                continue;
            }
            else if (IsNewLineTag(item))
            {
                if (includeNewlineTags)
                    currentCopyIndex = CopyIntoBuffer(buffer, currentCopyIndex, "<n/>", false);
                else
                    currentCopyIndex = CopyIntoBuffer(buffer, currentCopyIndex, " ", false);

                continue;
            }
            else if (eval && IsSpaceTag(item))
            {
                currentCopyIndex = CopyIntoBuffer(buffer, currentCopyIndex, " ", false);

                continue;
            }
            else if (item.StartsWith("</") && item[^1] == '>' && !item.EndsWith("/>")) // end tag
            {
                endTag = item;
                continue;
            }
            else if (item[0] == '<' && item[^1] == '>' && !item.EndsWith("/>")) // check if start tag
            {
                if (endTag.IsEmpty)
                {
                    currentCopyIndex = CopyIntoBuffer(buffer, currentCopyIndex, item, true);
                }
                else // dont save, empty tag
                {
                    endTag = [];
                }

                continue;
            }
            else if (item.StartsWith("~~") && item.EndsWith("~~") && double.TryParse(item.Trim('~'), CultureInfo.InvariantCulture, out double scaleValue))
            {
                currentCopyIndex = CopyIntoBuffer(buffer, currentCopyIndex, GetPerLevelLocale(scaleValue), false);

                continue;
            }
            else if (!endTag.IsEmpty)
            {
                currentCopyIndex = CopyIntoBuffer(buffer, currentCopyIndex, endTag, true);
                endTag = [];
            }

            currentCopyIndex = CopyIntoBuffer(buffer, currentCopyIndex, item, false);
        }

        // since we filled the buffer from the end, it could have null chars at the beginning
        return buffer.TrimStart('\0').ToString();
    }

    private void NestedTagCleanUp(ReadOnlySpan<char> gameString, Range? startTag = null)
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
                            {
                                _textStack.Push(endTag.Value);
                            }
                            else
                            {
                                _textStack.Push(startTag!.Value);
                                _missingEndTagsByStackCount.Add(_textStack.Count);
                            }
                        }

                        _textStack.Push(tag.Value);

                        NestedTagCleanUp(gameString, tag);

                        // nested
                        if (startTag.HasValue)
                            _textStack.Push(startTag.Value);
                    }
                    else if (IsNewLineTag(gameString, tag.Value))
                    {
                        // nested
                        if (startTag.HasValue)
                        {
                            if (TryGetEndTag(gameString, startTag.Value, out Range? endTag))
                                _textStack.Push(endTag.Value);

                            _textStack.Push(tag.Value);
                            _textStack.Push(startTag.Value);
                        }
                        else
                        {
                            _textStack.Push(tag.Value);
                        }
                    }
                    else if (startTag is not null)
                    {
                        if (IsTagsMatch(gameString, startTag.Value, tag.Value))
                        {
                            _textStack.Push(tag.Value);

                            return;
                        }
                    }
                    else if (IsSelfCloseTag(gameString, tag.Value))
                    {
                        _textStack.Push(tag.Value);
                    }
                }
                else
                {
                    _textStack.Pop();
#if DEBUG
                    PushNormalText(gameString);
#else
                    PushNormalText();
#endif
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
            bool missingEndTag = _textStack.Count > 0 && startTag.HasValue && _textStack.Peek().Equals(startTag);

#if DEBUG
            PushNormalText(gameString);
#else
            PushNormalText();
#endif
            if (missingEndTag)
            {
                _textStack.Push(startTag!.Value);
                _missingEndTagsByStackCount.Add(_textStack.Count);
            }
        }
    }

    private void PlainTextTagCleanup(ReadOnlySpan<char> gameString, bool includeScaling)
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
                if (TryParseTag(gameString, out Range? tag, out _))
                {
                    if (IsNewLineTag(gameString, tag.Value) || IsSpaceTag(gameString, tag.Value))
                        _textStack.Push(tag.Value);
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
                    if (includeScaling)
                    {
                        _textStack.Push(tag.Value);
                        _scaleValueByStackCount.Add(_textStack.Count);
                    }
                }
                else
                {
                    _textStack.Pop();
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
                if (TryParseErrorTag(gameString, out _))
                {
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
#if DEBUG
            PushNormalText(gameString);
#else
            PushNormalText();
#endif
        }
    }

    private void ColoredTextTagCleanup(ReadOnlySpan<char> gameString, bool includeScaling)
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
                if (TryParseTag(gameString, out Range? tag, out _))
                {
                    _textStack.Push(tag.Value);
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
                    if (includeScaling)
                    {
                        _textStack.Push(tag.Value);
                        _scaleValueByStackCount.Add(_textStack.Count);
                    }
                }
                else
                {
                    _textStack.Pop();
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
                if (TryParseErrorTag(gameString, out _))
                {
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
#if DEBUG
            PushNormalText(gameString);
#else
            PushNormalText();
#endif
        }
    }

#if DEBUG
    private void PushNormalText(ReadOnlySpan<char> gameString)
    {
        int normalTextLength = _index - _startingIndex;
        if (normalTextLength > 0)
        {
            ReadOnlySpan<char> temp = gameString.Slice(_startingIndex, normalTextLength);

            _textStack.Push(new Range(_startingIndex, _index));
        }
    }
#else
    private void PushNormalText()
    {
        int normalTextLength = _index - _startingIndex;
        if (normalTextLength > 0)
        {
            _textStack.Push(new Range(_startingIndex, _index));
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
        int spaceIndex = startTagSpan.IndexOf(' ');
        ReadOnlySpan<char> tagTypeSpan;

        if (spaceIndex > -1)
            tagTypeSpan = startTagSpan[..spaceIndex].TrimStart('<');
        else
            tagTypeSpan = startTagSpan.Trim("<>");

        int indexOfEndTag = currentTextSpan.IndexOf($"</{tagTypeSpan}>", StringComparison.OrdinalIgnoreCase);

        if (indexOfEndTag > -1)
        {
            endTag = new Range(_index + indexOfEndTag, _index + indexOfEndTag + tagTypeSpan.Length + 3);
            return true;
        }

        return false;
    }

    private int GetSizeOfBuffer(ReadOnlySpan<char> gameString)
    {
        int count = _textStack.Count;
        int sum = 0;

        foreach (Range item in _textStack)
        {
            if (_scaleValueByStackCount.Contains(count))
            {
                sum += 32;
                //ReadOnlySpan<char> text = gameString[item];
                //if (double.TryParse(text.Trim('~'), StormLocaleData.GetCultureInfo(StormLocale.ENUS), out double value))
                //{
                //    sum += GetPerLevelLocale(value).Length;
                //}
            }
            else
            {
                sum += item.End.Value - item.Start.Value;
            }

            count--;
        }

        return sum += _missingEndTagsByStackCount.Count;
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
