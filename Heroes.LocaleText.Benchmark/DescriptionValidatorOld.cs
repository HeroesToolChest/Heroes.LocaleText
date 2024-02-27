//using System.Diagnostics.CodeAnalysis;
//using System.Runtime.CompilerServices;
//using System.Text;

//namespace Heroes.Models;

///// <summary>
///// Provides methods to validate gamestrings and modify them into different verbiage.
///// </summary>
//public class DescriptionValidatorOld
//{
//    private readonly Localization _localization = Localization.ENUS;
//    private readonly ReadOnlyMemory<char> _gameStringMemory;

//    private readonly Stack<string> _textStack = new(101);

//    private int _iterator;

//    private DescriptionValidator(string gameString, Localization scaleLocale = Localization.ENUS)
//    {
//        if (string.IsNullOrEmpty(gameString))
//            gameString = string.Empty;

//        _localization = scaleLocale;

//        _gameStringMemory = gameString.AsMemory();

//        if (_gameStringMemory.Span.StartsWith("<li/>"))
//            _gameStringMemory = _gameStringMemory.TrimStart("<li/>");
//    }

//    /// <summary>
//    /// Takes a gamestring and removes unmatched and modifies nested tags into unnested tags.
//    /// </summary>
//    /// <param name="gameString">The gamestring text.</param>
//    /// <returns>A modified gamestring.</returns>
//    public static string Validate(string gameString)
//    {
//        return new DescriptionValidator(gameString).Validate();
//    }

//    /// <summary>
//    /// Returns a plain text string without any tags.
//    /// </summary>
//    /// <param name="gameString">The gamestring text.</param>
//    /// <param name="includeNewLineTags">If true, includes the newline tags.</param>
//    /// <param name="includeScaling">If true, includes the scaling info.</param>
//    /// <param name="scaleLocale">Locale for the per level string.</param>
//    /// <returns>A modified gamestring.</returns>
//    public static string GetPlainText(string gameString, bool includeNewLineTags, bool includeScaling, Localization scaleLocale = Localization.ENUS)
//    {
//        return new DescriptionValidator(gameString, scaleLocale).ParsePlainText(includeNewLineTags, includeScaling);
//    }

//    /// <summary>
//    /// Returns the string with all tags.
//    /// </summary>
//    /// <param name="gameString">The gamestring text.</param>
//    /// <param name="includeScaling">If true, includes the scaling info.</param>
//    /// <param name="scaleLocale">Locale for the per level string.</param>
//    /// <returns>A modified gamestring.</returns>
//    public static string GetColoredText(string gameString, bool includeScaling, Localization scaleLocale = Localization.ENUS)
//    {
//        return new DescriptionValidator(gameString, scaleLocale).ParseColoredText(includeScaling);
//    }

//    // checks if the end tag matches the start tag
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static bool MatchElement(ReadOnlySpan<char> startTag, ReadOnlySpan<char> endTag)
//    {
//        if (startTag.IsEmpty)
//            return false;

//        ReadOnlySpan<char> endTagSpan = endTag.TrimEnd('>').TrimStart('<').TrimStart('/');
//        ReadOnlySpan<char> firstPart;

//        int spaceIndex = startTag.IndexOf(' ');

//        if (spaceIndex > -1)
//            firstPart = startTag.Slice(0, spaceIndex).TrimStart('<');
//        else
//            firstPart = startTag.TrimStart('<');

//        return firstPart.SequenceEqual(endTagSpan);
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "to lower needed")]
//    private static string CreateEndTag(ReadOnlySpan<char> startTag)
//    {
//        startTag = startTag.TrimStart('<');
//        int spaceDelimeter = startTag.IndexOf(' ');

//        if (spaceDelimeter > 0)
//        {
//            ReadOnlySpan<char> firstPart = startTag.Slice(0, spaceDelimeter);
//            return "</" + firstPart.ToString().ToLowerInvariant() + ">";
//        }
//        else
//        {
//            return "</" + startTag.ToString().ToLowerInvariant();
//        }
//    }

//    // replaces double space or more into single space
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void RemoveDoublePlusSpaces(ref Span<char> span)
//    {
//        int position = 0;
//        int i = 0;
//        for (; i < span.Length && position < span.Length; i++, position++)
//        {
//            while (span[position] == ' ' && i < span.Length && span[position + 1] == ' ')
//            {
//                position++;
//            }

//            span[i] = span[position];
//        }

//        span = span.Slice(0, position - (position - i));
//    }

//    // replaces double space or more into single space and lowers the char
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void RemoveDoublePlusSpacesAndLower(ref Span<char> span)
//    {
//        int position = 0;
//        int i = 0;
//        for (; i < span.Length && position < span.Length; i++, position++)
//        {
//            while (span[position] == ' ' && i < span.Length && span[position + 1] == ' ')
//            {
//                position++;
//            }

//            span[i] = char.ToLowerInvariant(span[position]);
//        }

//        span = span.Slice(0, position - (position - i));
//    }

//    // lowers all the chars
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void LowerSpan(ref Span<char> span)
//    {
//        for (int i = 0; i < span.Length; i++)
//        {
//            span[i] = char.ToLowerInvariant(span[i]);
//        }
//    }

//    private string Validate()
//    {
//        ValidateGameString(string.Empty);

//        StringBuilder sb = new();

//        if (_textStack.Count < 1)
//            return string.Empty;

//        string endTag = string.Empty;
//        string firstItem = _textStack.Peek();

//        // remove unmatched start tag
//        if (firstItem.StartsWith("<", StringComparison.OrdinalIgnoreCase) && firstItem.EndsWith(">", StringComparison.OrdinalIgnoreCase) &&
//            !firstItem.EndsWith("/>", StringComparison.OrdinalIgnoreCase) && !firstItem.StartsWith("</", StringComparison.OrdinalIgnoreCase))
//            _textStack.Pop();

//        // remove empty elements
//        while (_textStack.Count > 0)
//        {
//            string item = _textStack.Pop();

//            if (item.StartsWith("</", StringComparison.OrdinalIgnoreCase) && item.EndsWith(">", StringComparison.OrdinalIgnoreCase) && !item.EndsWith("/>", StringComparison.OrdinalIgnoreCase)) // end tag
//            {
//                endTag = item;
//                continue;
//            }
//            else if (item.StartsWith("<", StringComparison.OrdinalIgnoreCase) && item.EndsWith(">", StringComparison.OrdinalIgnoreCase) && !item.EndsWith("/>", StringComparison.OrdinalIgnoreCase)) // check if start tag
//            {
//                if (string.IsNullOrEmpty(endTag))
//                {
//                    sb.Insert(0, item);
//                    continue;
//                }
//                else // dont save, empty tag
//                {
//                    endTag = string.Empty;
//                    continue;
//                }
//            }
//            else if (!string.IsNullOrEmpty(endTag))
//            {
//                sb.Insert(0, endTag);
//                endTag = string.Empty;
//            }

//            sb.Insert(0, item);
//        }

//        return sb.ToString();
//    }

//    // modifies game string, remove unmatched tags, nested tags
//    private void ValidateGameString(string startTag)
//    {
//        ReadOnlySpan<char> gameStringSpan = _gameStringMemory.Span;
//        StringBuilder sb = new();

//        for (; _iterator < gameStringSpan.Length; _iterator++)
//        {
//            if (gameStringSpan[_iterator] == '<' && gameStringSpan[_iterator + 1] != ' ')
//            {
//                if (sb.Length > 0)
//                {
//                    _textStack.Push(sb.ToString());
//                    sb = new StringBuilder();
//                }

//                if (TryParseTag(out Span<char> tag, out bool isStartTag))
//                {
//                    string tagAsString = tag.ToString();
//                    if (isStartTag)
//                    {
//                        _iterator++;

//                        // nested
//                        if (!string.IsNullOrEmpty(startTag))
//                            _textStack.Push(CreateEndTag(startTag));

//                        _textStack.Push(tagAsString);
//                        ValidateGameString(tagAsString);

//                        if (!string.IsNullOrEmpty(startTag))
//                            _textStack.Push(startTag);

//                        continue;
//                    }
//                    else if (tagAsString.Equals("<n/>", StringComparison.OrdinalIgnoreCase) || tagAsString.Equals("</n>", StringComparison.OrdinalIgnoreCase)) // line breakers
//                    {
//                        tagAsString = "<n/>";

//                        // nested
//                        if (!string.IsNullOrEmpty(startTag))
//                        {
//                            _textStack.Push(CreateEndTag(startTag));
//                            _textStack.Push(tagAsString);
//                            _textStack.Push(startTag);
//                        }
//                        else
//                        {
//                            _textStack.Push(tagAsString);
//                        }

//                        continue;
//                    }
//                    else // end tag
//                    {
//                        if (MatchElement(startTag, tag))
//                        {
//                            if (_textStack.Peek() != startTag)
//                                _textStack.Push(tagAsString);
//                            else
//                                _textStack.Pop();
//                            return;
//                        }
//                        else if (tag.Length > 4 && tagAsString.EndsWith("/>", StringComparison.OrdinalIgnoreCase)) // self close tag
//                        {
//                            _textStack.Push(tagAsString);
//                            continue;
//                        }
//                        else
//                        {
//                            continue;
//                        }
//                    }
//                }
//            }

//            if (_iterator >= gameStringSpan.Length)
//                break;

//            sb.Append(gameStringSpan[_iterator]);
//        }

//        if (sb.Length > 0)
//        {
//            if (_textStack.Count > 0 && _textStack.Peek() == startTag)
//                _textStack.Pop();

//            _textStack.Push(sb.ToString());
//        }
//    }

//    // removes all tags
//    private string ParsePlainText(bool includeNewlineTags, bool includeScaling)
//    {
//        ReadOnlySpan<char> gameStringSpan = _gameStringMemory.Span;
//        StringBuilder sb = new();

//        for (; _iterator < gameStringSpan.Length; _iterator++)
//        {
//            if (gameStringSpan[_iterator] == '<')
//            {
//                if (sb.Length > 0)
//                {
//                    _textStack.Push(sb.ToString());
//                    sb = new StringBuilder();
//                }

//                if (TryParseTag(out Span<char> tag, out _))
//                {
//                    string tagAsString = tag.ToString();
//                    if (tagAsString.Equals("<n/>", StringComparison.OrdinalIgnoreCase))
//                    {
//                        if (includeNewlineTags)
//                            _textStack.Push(tagAsString);
//                        else
//                            _textStack.Push(" ");
//                    }
//                    else if (tagAsString.Equals("<sp/>", StringComparison.OrdinalIgnoreCase))
//                    {
//                        _textStack.Push(" ");
//                    }

//                    continue;
//                }
//            }
//            else if (gameStringSpan[_iterator] == '~' && _iterator + 1 < gameStringSpan.Length && gameStringSpan[_iterator + 1] == '~')
//            {
//                _textStack.Push(sb.ToString());
//                sb = new StringBuilder();

//                if (TryParseScalingTag(out string scaleText, includeScaling))
//                {
//                    _textStack.Push(scaleText);
//                }

//                continue;
//            }
//            else if (gameStringSpan[_iterator] == '#' && _iterator + 1 < gameStringSpan.Length && gameStringSpan[_iterator + 1] == '#')
//            {
//                _textStack.Push(sb.ToString());
//                sb = new StringBuilder();

//                if (TryParseErrorTag(out string value))
//                {
//                    _textStack.Push(value);
//                }

//                continue;
//            }

//            if (_iterator >= gameStringSpan.Length)
//                break;

//            sb.Append(gameStringSpan[_iterator]);
//        }

//        if (sb.Length > 0)
//            _textStack.Push(sb.ToString());

//        sb = new StringBuilder();

//        while (_textStack.Count > 0)
//        {
//            sb.Insert(0, _textStack.Pop());
//        }

//        return sb.ToString().Trim();
//    }

//    // keeps colored tags and new lines
//    private string ParseColoredText(bool includeScaling)
//    {
//        ReadOnlySpan<char> gameStringSpan = _gameStringMemory.Span;
//        StringBuilder sb = new();

//        for (; _iterator < gameStringSpan.Length; _iterator++)
//        {
//            if (gameStringSpan[_iterator] == '<')
//            {
//                if (sb.Length > 0)
//                {
//                    _textStack.Push(sb.ToString());
//                    sb = new StringBuilder();
//                }

//                if (TryParseTag(out Span<char> tag, out _))
//                {
//                    _textStack.Push(tag.ToString());

//                    continue;
//                }
//            }
//            else if (gameStringSpan[_iterator] == '~' && _iterator + 1 < gameStringSpan.Length && gameStringSpan[_iterator + 1] == '~')
//            {
//                _textStack.Push(sb.ToString());
//                sb = new StringBuilder();

//                if (TryParseScalingTag(out string scaleText, includeScaling))
//                {
//                    _textStack.Push(scaleText);
//                }

//                continue;
//            }
//            else if (gameStringSpan[_iterator] == '#' && _iterator + 1 < gameStringSpan.Length && gameStringSpan[_iterator + 1] == '#')
//            {
//                _textStack.Push(sb.ToString());
//                sb = new StringBuilder();

//                if (TryParseErrorTag(out string value))
//                {
//                    _textStack.Push(value);
//                }

//                continue;
//            }

//            if (_iterator >= gameStringSpan.Length)
//                break;

//            sb.Append(gameStringSpan[_iterator]);
//        }

//        if (sb.Length > 0)
//            _textStack.Push(sb.ToString());

//        sb = new StringBuilder();

//        while (_textStack.Count > 0)
//        {
//            sb.Insert(0, _textStack.Pop());
//        }

//        return sb.ToString().Trim();
//    }

//    // get whole tag, determine if it's a start tag
//    private bool TryParseTag(out Span<char> tag, out bool isStartTag)
//    {
//        ReadOnlySpan<char> gameStringSpan = _gameStringMemory.Span;
//        ReadOnlySpan<char> currentIterationSpan = gameStringSpan[_iterator..];

//        int startTagIndex = currentIterationSpan.IndexOf('<');
//        int endTagIndex = currentIterationSpan.IndexOf('>');

//        if (startTagIndex > -1 && endTagIndex > -1)
//            tag = new char[endTagIndex - startTagIndex + 1];
//        else
//            tag = new char[gameStringSpan.Length - _iterator];

//        int spanPosition = 0;

//        for (; _iterator < gameStringSpan.Length; _iterator++)
//        {
//            tag[spanPosition++] = gameStringSpan[_iterator];

//            if (gameStringSpan[_iterator] == '>')
//            {
//                int spaceIndex = tag.IndexOf(' ');
//                if (spaceIndex > -1)
//                {
//                    Span<char> tagType = tag.Slice(0, spaceIndex);
//                    LowerSpan(ref tagType);
//                }
//                else
//                {
//                    LowerSpan(ref tag);
//                }

//                // check if its a start tag
//                if (tag[1] != '/' && tag[^2] != '/')
//                    isStartTag = true;
//                else
//                    isStartTag = false;

//                RemoveDoublePlusSpaces(ref tag);
//                return true;
//            }
//        }

//        isStartTag = false;

//        RemoveDoublePlusSpacesAndLower(ref tag);
//        return false;
//    }

//    /// <summary>
//    /// Parse the scaling tag, removes the tag or replaces it.
//    /// </summary>
//    /// <param name="scaleText">The scale text.</param>
//    /// <param name="replace">If true, replace the tag, else return an empty string.</param>
//    private bool TryParseScalingTag(out string scaleText, bool replace)
//    {
//        ReadOnlySpan<char> gameStringSpan = _gameStringMemory.Span;

//        int startTagIndex = -1; // second of the ~~
//        int endTagIndex = -1; // first of the ~~

//        for (; _iterator < gameStringSpan.Length; _iterator++)
//        {
//            if (gameStringSpan[_iterator] == '~')
//            {
//                // check if next is also ~
//                if (_iterator + 1 < gameStringSpan.Length && gameStringSpan[_iterator + 1] == '~')
//                {
//                    if (startTagIndex < 0)
//                    {
//                        startTagIndex = ++_iterator + 1;
//                    }
//                    else if (endTagIndex < 0)
//                    {
//                        endTagIndex = _iterator;
//                        _iterator++;
//                    }
//                }
//            }

//            if (startTagIndex > 0 && endTagIndex > 0)
//            {
//                ReadOnlySpan<char> value = gameStringSpan[startTagIndex..endTagIndex];

//                if (replace)
//                    scaleText = $" ({GetPerLevelLocale(double.Parse(value) * 100)})";
//                else
//                    scaleText = string.Empty;

//                return true;
//            }
//        }

//        scaleText = string.Empty;
//        return false;
//    }

//    /// <summary>
//    /// Parses the error tag by removing it. Error tags are ##TEXT##.
//    /// </summary>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private bool TryParseErrorTag(out string value)
//    {
//        ReadOnlySpan<char> gameStringSpan = _gameStringMemory.Span;

//        value = string.Empty;

//        if (gameStringSpan.Contains("##ERROR##", StringComparison.OrdinalIgnoreCase))
//        {
//            _iterator += 8;
//            return true;
//        }

//        return false;
//    }

//    private string GetPerLevelLocale(double value)
//    {
//        if (_localization == Localization.DEDE)
//            return $"+{value:0.##}% pro Stufe";
//        else if (_localization == Localization.ENUS)
//            return $"+{value:0.##}% per level";
//        else if (_localization == Localization.ESES)
//            return $"+{value:0.##}% por nivel";
//        else if (_localization == Localization.ESMX)
//            return $"+{value:0.##}% por nivel";
//        else if (_localization == Localization.FRFR)
//            return $"+{value:0.##}% par niveau";
//        else if (_localization == Localization.ITIT)
//            return $"+{value:0.##}% per livello";
//        else if (_localization == Localization.KOKR)
//            return $"레벨 당 +{value:0.##}%";
//        else if (_localization == Localization.PLPL)
//            return $"+{value:0.##}% na poziom";
//        else if (_localization == Localization.PTBR)
//            return $"+{value:0.##}% por nível";
//        else if (_localization == Localization.RURU)
//            return $"+{value:0.##}% за уровень";
//        else if (_localization == Localization.ZHCN)
//            return $"每级 +{value:0.##}%";
//        else if (_localization == Localization.ZHTW)
//            return $"每級 +{value:0.##}%";
//        else
//            return $"{value:0.##}% per level";
//    }
//}
