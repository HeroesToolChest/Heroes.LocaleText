using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Heroes.LocaleText;

/// <summary>
/// Contains the information for a gamestring.
/// </summary>
public class GameStringText
{
    /// <summary>
    /// The error tag string.
    /// </summary>
    public const string ErrorTag = "##ERROR##";

    private readonly GameStringParser _descriptionParser;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _rawText;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _plainText;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _plainTextWithNewlines;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _plainTextWithScaling;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _plainTextWithScalingWithNewlines;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _coloredText;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _coloredTextWithScaling;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameStringText"/> class.
    /// </summary>
    /// <param name="text">A gamestring that has had its data references computed but hasn't been turned into readable text yet. Don't pass in the output of <see cref="PlainText"/> or <see cref="ColoredText"/>.</param>
    /// <param name="gameStringLocale">
    /// The localization of the <paramref name="text"/>. This is just used for the scaling text.
    /// The scaling value (e.g. <c>~~0,04~~</c>) may use this locale's decimal separator or a period, and the scaling text (e.g. <c>(+4% per level)</c>) is written for this locale.
    /// If the scaling values only use a period and the scaling text is not needed, then this may be left as the default.
    /// </param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, the <c>val</c> values of the style and constant tags are collected in <see cref="FontStyleValues"/> and <see cref="FontConstantValues"/>.
    /// If <see langword="false"/>, the values aren't collected, which makes parsing faster. The tags still appear in the output and font value replacements still work.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public GameStringText(string text, StormLocale gameStringLocale = StormLocale.ENUS, bool extractFontValues = false)
    {
        ArgumentNullException.ThrowIfNull(text);

        GameStringLocale = gameStringLocale;
        IsFontValuesExtracted = extractFontValues;

        _descriptionParser = GameStringParser.GetInstance(text, gameStringLocale, extractFontValues);
    }

    /// <summary>
    /// <para>Gets the raw text. Unmatched tags have been fixed and nested tags have been modified into unnested tags.</para>
    /// <para>Contains the xml tags, scaling data <c>~~x~~</c>, and newlines <c>&lt;n/&gt;</c>. It can also contain error tags <c>##ERROR##</c>.</para>
    /// <para>
    /// Tag names are lowercased, extra spaces in tags are removed, the newline tags <c>&lt;n&gt;</c> and <c>&lt;/n&gt;</c> become <c>&lt;n/&gt;</c>, and empty tags are removed.
    /// Any font value replacements are applied.
    /// </para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals &lt;c val="#TooltipNumbers"&gt;200&lt;/c&gt;&lt;c val="#ColorGray"&gt;~~0.04~~&lt;/c&gt; damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string RawText => _rawText ??= _descriptionParser.GetRawText();

    /// <summary>
    /// <para>Gets the gamestring as text only.</para>
    /// <para>No xml tags, scaling info, or newlines. Newlines are replaced with a single space.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals 200 damage. Does not affect minions.
    /// </para>
    /// </summary>
    public string PlainText => _plainText ??= _descriptionParser.GetPlainText(false, false);

    /// <summary>
    /// <para>Gets the gamestring as text only.</para>
    /// <para>Same as <see cref="PlainText"/> but keeps the newlines as <c>&lt;n/&gt;</c> tags.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals 200 damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string PlainTextWithNewlines => _plainTextWithNewlines ??= _descriptionParser.GetPlainText(true, false);

    /// <summary>
    /// <para>Gets the gamestring as text only.</para>
    /// <para>Same as <see cref="PlainText"/> but contains the scaling info <c>(+x% per level)</c>.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals 200 (+4% per level) damage. Does not affect minions.
    /// </para>
    /// </summary>
    public string PlainTextWithScaling => _plainTextWithScaling ??= _descriptionParser.GetPlainText(false, true);

    /// <summary>
    /// <para>Gets the gamestring as text only.</para>
    /// <para>Same as <see cref="PlainTextWithScaling"/> but keeps the newlines as <c>&lt;n/&gt;</c> tags.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals 200 (+4% per level) damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string PlainTextWithScalingWithNewlines => _plainTextWithScalingWithNewlines ??= _descriptionParser.GetPlainText(true, true);

    /// <summary>
    /// <para>Gets the gamestring with xml tags and newlines. Once the tags are rendered, this is what appears in-game for text and tooltips.</para>
    /// <para>No scaling info or error tags. Any font value replacements are applied.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals &lt;c val="#TooltipNumbers"&gt;200&lt;/c&gt; damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string ColoredText => _coloredText ??= _descriptionParser.GetColoredText(false);

    /// <summary>
    /// <para>Gets the gamestring with xml tags, newlines, and scaling info.</para>
    /// <para>Same as <see cref="ColoredText"/> but contains the scaling info <c>(+x% per level)</c>.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals &lt;c val="#TooltipNumbers"&gt;200&lt;/c&gt;&lt;c val="#ColorGray"&gt; (+4% per level)&lt;/c&gt; damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string ColoredTextWithScaling => _coloredTextWithScaling ??= _descriptionParser.GetColoredText(true);

    /// <summary>
    /// Gets the localization used for the gamestring.
    /// </summary>
    public StormLocale GameStringLocale { get; }

    /// <summary>
    /// Gets a value indicating whether the style and constant tag values are collected, as set by the <c>extractFontValues</c> constructor parameter.
    /// <para>
    /// If <see langword="true"/>, <see cref="FontStyleValues"/> and <see cref="FontConstantValues"/> will not be <see langword="null"/>.
    /// </para>
    /// </summary>
    [MemberNotNullWhen(true, nameof(FontStyleValues), nameof(FontConstantValues))]
    public bool IsFontValuesExtracted { get; }

    /// <summary>
    /// <para>Gets the set of style tag values used in the gamestring. The values are case-sensitive.</para>
    /// <para>If font value replacements have been added, the replaced values are listed instead of the original values.</para>
    /// <para>
    /// Example:<br/>
    /// With &lt;s val="StandardTooltipHeader"&gt;&lt;/s&gt; returns StandardTooltipHeader.
    /// </para>
    /// </summary>
    /// <value>A read-only set of style tag values, or <see langword="null"/> if <see cref="IsFontValuesExtracted"/> is <see langword="false"/>.</value>
    public IReadOnlySet<string>? FontStyleValues
    {
        get
        {
            if (!IsFontValuesExtracted)
                return null;

            // the values are extracted while parsing, no output string needs to be built
            _descriptionParser.EnsureParsed();

            return _descriptionParser.StyleTagVariables;
        }
    }

    /// <summary>
    /// <para>Gets the set of constant tag values used in the gamestring. The values are case-sensitive.</para>
    /// <para>If font value replacements have been added, the replaced values are listed instead of the original values.</para>
    /// <para>
    /// Example:<br/>
    /// With &lt;c val="#TooltipNumbers"&gt;&lt;/c&gt; returns #TooltipNumbers.
    /// </para>
    /// </summary>
    /// <value>A read-only set of constant tag values, or <see langword="null"/> if <see cref="IsFontValuesExtracted"/> is <see langword="false"/>.</value>
    public IReadOnlySet<string>? FontConstantValues
    {
        get
        {
            if (!IsFontValuesExtracted)
                return null;

            // the values are extracted while parsing, no output string needs to be built
            _descriptionParser.EnsureParsed();

            return _descriptionParser.StyleConstantTagVariables;
        }
    }

    /// <summary>
    /// <para>Adds a dictionary of values that will be replaced by new values. Replaces the <c>val</c> values of either the style or the constant tags, based on <paramref name="fontTagType"/>.</para>
    /// <para>
    /// The replacements show up in <see cref="RawText"/>, <see cref="ColoredText"/>, and <see cref="ColoredTextWithScaling"/>, and the replaced values are listed in <see cref="FontStyleValues"/> or <see cref="FontConstantValues"/>.
    /// The plain text properties have no tags, so they aren't affected.
    /// </para>
    /// </summary>
    /// <param name="fontTagType">The tag type for the replacement of the values.</param>
    /// <param name="preserveValues">If <see langword="true"/>, creates a new <c>hlt-name</c> attribute with the name of the replaced (the original) value. A tag that already has an <c>hlt-name</c> attribute keeps that one instead.</param>
    /// <param name="newValuesByValue">A dictionary of values and their replacement values. The values are matched case-sensitively.</param>
    /// <returns>The current <see cref="GameStringText"/> instance.</returns>
    public GameStringText AddFontValueReplacements(FontTagType fontTagType, bool preserveValues, IDictionary<string, string> newValuesByValue)
    {
        foreach (var item in newValuesByValue)
        {
            AddFontValueReplacementInternal(item.Key, item.Value, fontTagType, preserveValues);
        }

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// <para>Adds a collection of values that will be replaced by new values. Replaces the <c>val</c> values of either the style or the constant tags, based on <paramref name="fontTagType"/>.</para>
    /// <para>
    /// The replacements show up in <see cref="RawText"/>, <see cref="ColoredText"/>, and <see cref="ColoredTextWithScaling"/>, and the replaced values are listed in <see cref="FontStyleValues"/> or <see cref="FontConstantValues"/>.
    /// The plain text properties have no tags, so they aren't affected.
    /// </para>
    /// </summary>
    /// <param name="fontTagType">The tag type for the replacement of the values.</param>
    /// <param name="preserveValues">If <see langword="true"/>, creates a new <c>hlt-name</c> attribute with the name of the replaced (the original) value. A tag that already has an <c>hlt-name</c> attribute keeps that one instead.</param>
    /// <param name="newValuesByValue">A collection of values and their replacement values. The values are matched case-sensitively.</param>
    /// <returns>The current <see cref="GameStringText"/> instance.</returns>
    public GameStringText AddFontValueReplacements(FontTagType fontTagType, bool preserveValues, IEnumerable<(string Value, string Replacement)> newValuesByValue)
    {
        foreach ((string value, string replacement) in newValuesByValue)
        {
            AddFontValueReplacementInternal(value, replacement, fontTagType, preserveValues);
        }

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// <para>Adds a collection of values that will be replaced by new values. Replaces the <c>val</c> values of either the style or the constant tags, based on <paramref name="fontTagType"/>.</para>
    /// <para>
    /// The replacements show up in <see cref="RawText"/>, <see cref="ColoredText"/>, and <see cref="ColoredTextWithScaling"/>, and the replaced values are listed in <see cref="FontStyleValues"/> or <see cref="FontConstantValues"/>.
    /// The plain text properties have no tags, so they aren't affected.
    /// </para>
    /// </summary>
    /// <param name="fontTagType">The tag type for the replacement of the values.</param>
    /// <param name="preserveValues">If <see langword="true"/>, creates a new <c>hlt-name</c> attribute with the name of the replaced (the original) value. A tag that already has an <c>hlt-name</c> attribute keeps that one instead.</param>
    /// <param name="newValuesByValue">A collection of values and their replacement values. The values are matched case-sensitively.</param>
    /// <returns>The current <see cref="GameStringText"/> instance.</returns>
    public GameStringText AddFontValueReplacements(FontTagType fontTagType, bool preserveValues, params (string Value, string Replacement)[] newValuesByValue)
    {
        foreach ((string value, string replacement) in newValuesByValue)
        {
            AddFontValueReplacementInternal(value, replacement, fontTagType, preserveValues);
        }

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// <para>Adds a value that will be replaced by a new value. Replaces the <c>val</c> value of either the style or the constant tags, based on <paramref name="fontTagType"/>.</para>
    /// <para>
    /// The replacement shows up in <see cref="RawText"/>, <see cref="ColoredText"/>, and <see cref="ColoredTextWithScaling"/>, and the replaced value is listed in <see cref="FontStyleValues"/> or <see cref="FontConstantValues"/>.
    /// The plain text properties have no tags, so they aren't affected.
    /// </para>
    /// </summary>
    /// <param name="value">The value of the <c>val</c> attribute of the tag. It's matched case-sensitively.</param>
    /// <param name="replacement">The new value that will replace <paramref name="value"/>.</param>
    /// <param name="fontTagType">The tag type for the replacement of the <paramref name="value"/>.</param>
    /// <param name="preserveValue">If <see langword="true"/>, creates a new <c>hlt-name</c> attribute with the name of the replaced (the original) <paramref name="value"/>. A tag that already has an <c>hlt-name</c> attribute keeps that one instead.</param>
    /// <returns>The current <see cref="GameStringText"/> instance.</returns>
    public GameStringText AddFontValueReplacement(string value, string replacement, FontTagType fontTagType, bool preserveValue = false)
    {
        ApplyFontValueReplacement(value, replacement, fontTagType, preserveValue);

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// <para>Removes the <c>hlt-name</c> attribute from the style tags and optionally undoes the replacement of the value for the style tags.</para>
    /// <para>Only the <c>hlt-name</c> attributes that are already in the gamestring are removed. The ones created by font value replacements that preserve the value are kept.</para>
    /// <para>Font value replacements are applied after the removal, to the <c>val</c> value that is left. The order of the calls doesn't matter.</para>
    /// </summary>
    /// <param name="undoFontValueReplacement">If <see langword="true"/>, undoes the replacement of the value for the style tags (it will be set to the removed <c>hlt-name</c> value).</param>
    /// <returns>The current <see cref="GameStringText"/> instance.</returns>
    public GameStringText RemoveHltNameForStyleTags(bool undoFontValueReplacement)
    {
        _descriptionParser.RemoveHltNameFromStyleTags = true;
        _descriptionParser.UndoHltNameReplacementForStyleTags = undoFontValueReplacement;

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// <para>Removes the <c>hlt-name</c> attribute from the constant tags and optionally undoes the replacement of the value for the constant tags.</para>
    /// <para>Only the <c>hlt-name</c> attributes that are already in the gamestring are removed. The ones created by font value replacements that preserve the value are kept.</para>
    /// <para>Font value replacements are applied after the removal, to the <c>val</c> value that is left. The order of the calls doesn't matter.</para>
    /// </summary>
    /// <param name="undoFontValueReplacement">If <see langword="true"/>, undoes the replacement of the value for the constant tags (it will be set to the removed <c>hlt-name</c> value).</param>
    /// <returns>The current <see cref="GameStringText"/> instance.</returns>
    public GameStringText RemoveHltNameForConstantTags(bool undoFontValueReplacement)
    {
        _descriptionParser.RemoveHltNameFromConstantTags = true;
        _descriptionParser.UndoHltNameReplacementForConstantTags = undoFontValueReplacement;

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// Returns the same as <see cref="PlainTextWithScaling"/>.
    /// </summary>
    /// <returns>The same as <see cref="PlainTextWithScaling"/>.</returns>
    public override string ToString()
    {
        return PlainTextWithScaling;
    }

    private void AddFontValueReplacementInternal(string value, string replacement, FontTagType fontTagType, bool preserveValue = false)
    {
        ApplyFontValueReplacement(value, replacement, fontTagType, preserveValue);
    }

    private void ApplyFontValueReplacement(string value, string replacement, FontTagType fontTagType, bool preserveValue)
    {
        if (fontTagType == FontTagType.Constant)
        {
            _descriptionParser.AddStyleConstantVarsWithReplacement(value, replacement, preserveValue);
            ReplaceFontValue(_descriptionParser.StyleConstantTagVariables, value, replacement);
        }
        else if (fontTagType == FontTagType.Style)
        {
            _descriptionParser.AddStyleVarsWithReplacement(value, replacement, preserveValue);
            ReplaceFontValue(_descriptionParser.StyleTagVariables, value, replacement);
        }
    }

    // keeps the extracted font values in sync with the replacements
    private void ReplaceFontValue(HashSet<string>? fontValues, string value, string replacement)
    {
        if (fontValues is null)
            return;

        // the values get extracted while parsing, so parse first, otherwise a later parse would add back the original value
        _descriptionParser.EnsureParsed();

        // only replace a value that is in the gamestring
        if (fontValues.Remove(value))
            fontValues.Add(replacement);
    }

    private void ClearDescriptionsCaches()
    {
        _rawText = null;
        _coloredText = null;
        _coloredTextWithScaling = null;
    }
}
