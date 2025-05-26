using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Heroes.LocaleText;

/// <summary>
/// Contains the information for tooltip descriptions.
/// </summary>
public class TooltipDescription
{
    /// <summary>
    /// The error tag string.
    /// </summary>
    public const string ErrorTag = "##ERROR##";

    private readonly DescriptionParser _descriptionParser;
    private HashSet<string>? _fontStyleValues;
    private HashSet<string>? _fontStyleConstantValues;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _rawDescription;

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
    /// Initializes a new instance of the <see cref="TooltipDescription"/> class.
    /// </summary>
    /// <param name="text">A parsed description that has not been modified into a readable verbiage (e.g. <see cref="PlainText"/> or <see cref="ColoredText"/> from this class should not be used).</param>
    /// <param name="gameStringLocale">The localization of the <paramref name="text"/>.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, then the font style and constant tags will have their val values saved in <see cref="FontStyleValues"/> and  <see cref="FontStyleConstantValues"/>.
    /// If not needing the output with color tags, then set to <see langword="false"/> for faster parsing performance.
    /// </param>
    public TooltipDescription(string text, StormLocale gameStringLocale = StormLocale.ENUS, bool extractFontValues = false)
    {
        ArgumentNullException.ThrowIfNull(text);

        GameStringLocale = gameStringLocale;
        IsFontValuesExtracted = extractFontValues;

        _descriptionParser = DescriptionParser.GetInstance(text, gameStringLocale, extractFontValues);
    }

    /// <summary>
    /// <para>Gets the raw description. Unmatched tags have been fixed and nested tag have been modified into unnested tags.</para>
    /// <para>Contains the color tags &lt;c val=\"#TooltipNumbers\"&gt;&lt;/c&gt;, scaling data ~~x~~, and newlines &lt;n/&gt;. It can also contain error tags ##ERROR##.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals &lt;c val=\"#TooltipNumbers\"&gt;200~~0.04~~&lt;/c&gt; damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string RawDescription => _rawDescription ??= _descriptionParser.GetRawDescription();

    /// <summary>
    /// <para>Gets the description with text only.</para>
    /// <para>No color tags, scaling info, or newlines. Newlines are replaced with a single space.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals 200 damage. Does not affect minions.
    /// </para>
    /// </summary>
    public string PlainText => _plainText ??= _descriptionParser.GetPlainText(false, false);

    /// <summary>
    /// <para>Gets the validated description with text only.</para>
    /// <para>Same as <see cref="PlainText"/> but contains newlines.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals 200 damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string PlainTextWithNewlines => _plainTextWithNewlines ??= _descriptionParser.GetPlainText(true, false);

    /// <summary>
    /// <para>Gets the description with text only.</para>
    /// <para>Same as <see cref="PlainText"/> but contains the scaling info (+x% per level).</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals 200 (+4% per level) damage. Does not affect minions.
    /// </para>
    /// </summary>
    public string PlainTextWithScaling => _plainTextWithScaling ??= _descriptionParser.GetPlainText(false, true);

    /// <summary>
    /// <para>Gets the description with text only.</para>
    /// <para>Same as <see cref="PlainTextWithScaling"/> but contains the newlines.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals 200 (+4% per level) damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string PlainTextWithScalingWithNewlines => _plainTextWithScalingWithNewlines ??= _descriptionParser.GetPlainText(true, true);

    /// <summary>
    /// <para>Gets the description with colored tags and new lines, when parsed this is what appears ingame for tooltips.</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals &lt;c val=\"#TooltipNumbers\"&gt;200&lt;/c&gt; damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string ColoredText => _coloredText ??= _descriptionParser.GetColoredText(false);

    /// <summary>
    /// <para>Gets the description with colored tags, newlines, and scaling info.</para>
    /// <para>Same as <see cref="ColoredText"/> but contains the scaling info (+x% per level).</para>
    /// <para>
    /// Example:<br/>
    /// Fires a laser that deals &lt;c val=\"#TooltipNumbers\"&gt;200 (+4% per level)&lt;/c&gt; damage.&lt;n/&gt;Does not affect minions.
    /// </para>
    /// </summary>
    public string ColoredTextWithScaling => _coloredTextWithScaling ??= _descriptionParser.GetColoredText(true);

    /// <summary>
    /// Gets the localization used for the description text.
    /// </summary>
    public StormLocale GameStringLocale { get; }

    /// <summary>
    /// Gets a value indicating whether the font style and constant values have been extracted.
    /// <para>
    /// If <see langword="true"/>, <see cref="FontStyleValues"/> and <see cref="FontStyleConstantValues"/> will not be <see langword="null"/>.
    /// </para>
    /// </summary>
    [MemberNotNullWhen(true, nameof(FontStyleValues), nameof(FontStyleConstantValues))]
    public bool IsFontValuesExtracted { get; }

    /// <summary>
    /// <para>Gets a collection of text style values used in the tooltip description.</para>
    /// <para>
    /// Example:<br/>
    /// With &lt;s val=\"StandardTooltipHeader\"&gt;&lt;/s&gt; returns StandardTooltipHeader.
    /// </para>
    /// </summary>
    /// <returns>A collection of text style values or <see langword="null"/> if the <see cref="IsFontValuesExtracted"/> is <see langword="false"/>.</returns>
    public IEnumerable<string>? FontStyleValues
    {
        get
        {
            if (_rawDescription is null)
                _ = RawDescription; // trigger the parsing

            return _fontStyleValues ??= _descriptionParser.StyleTagVariables;
        }
    }

    /// <summary>
    /// <para>Gets a collection of text style constant values used in the tooltip description.</para>
    /// <para>
    /// Example:<br/>
    /// With &lt;c val=\"#TooltipNumbers\"&gt;&lt;/c&gt; returns #TooltipNumbers.
    /// </para>
    /// </summary>
    /// <returns>A collection of text style constant values or <see langword="null"/> if the <see cref="IsFontValuesExtracted"/> is <see langword="false"/>.</returns>
    public IEnumerable<string>? FontStyleConstantValues
    {
        get
        {
            if (_rawDescription is null)
                _ = RawDescription; // trigger the parsing

            return _fontStyleConstantValues ??= _descriptionParser.StyleConstantTagVariables;
        }
    }

    /// <summary>
    /// Adds a dictionary of values that will be replaced by new values. Used to replace the variables in the font style tags.
    /// </summary>
    /// <param name="fontTagType">The tag type for the replacement of the values.</param>
    /// <param name="preserveValues">If <see langword="true"/> creates a new attribute 'hlt-name' with the name of the replaced (the original) value.</param>
    /// <param name="newValuesByValue">A dictionary of values and their replacement values. Is case-sensitive.</param>
    /// <returns>The current <see cref="TooltipDescription"/> instance.</returns>
    public TooltipDescription AddFontValueReplacements(FontTagType fontTagType, bool preserveValues, IDictionary<string, string> newValuesByValue)
    {
        foreach (var item in newValuesByValue)
        {
            AddFontValueReplacementInternal(item.Key, item.Value, fontTagType, preserveValues);
        }

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// Adds a collection of values that will be replaced by new values. Used to replace the variables in the font style tags.
    /// </summary>
    /// <param name="fontTagType">The tag type for the replacement of the values.</param>
    /// <param name="preserveValues">If <see langword="true"/> creates a new attribute 'hlt-name' with the name of the replaced (the original) value.</param>
    /// <param name="newValuesByValue">A collection of values and their replacement values. Is case-sensitive.</param>
    /// <returns>The current <see cref="TooltipDescription"/> instance.</returns>
    public TooltipDescription AddFontValueReplacements(FontTagType fontTagType, bool preserveValues, IEnumerable<(string Value, string Replacement)> newValuesByValue)
    {
        foreach ((string value, string replacement) in newValuesByValue)
        {
            AddFontValueReplacementInternal(value, replacement, fontTagType, preserveValues);
        }

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// Adds a collection of values that will be replaced by new values. Used to replace the variables in the font style tags.
    /// </summary>
    /// <param name="fontTagType">The tag type for the replacement of the values.</param>
    /// <param name="preserveValues">If <see langword="true"/> creates a new attribute 'hlt-name' with the name of the replaced (the original) value.</param>
    /// <param name="newValuesByValue">A collection of values and their replacement values. Is case-sensitive.</param>
    /// <returns>The current <see cref="TooltipDescription"/> instance.</returns>
    public TooltipDescription AddFontValueReplacements(FontTagType fontTagType, bool preserveValues, params (string Value, string Replacement)[] newValuesByValue)
    {
        foreach ((string value, string replacement) in newValuesByValue)
        {
            AddFontValueReplacementInternal(value, replacement, fontTagType, preserveValues);
        }

        ClearDescriptionsCaches();

        return this;
    }

    /// <summary>
    /// Adds a value that will be replaced by a new value.  Used to replace the variables in the font style tags.
    /// </summary>
    /// <param name="value">The value of the val attribute of the tag. Is case-sensitive.</param>
    /// <param name="replacement">The new value that will replace <paramref name="value"/>. Is case-sensitive.</param>
    /// <param name="fontTagType">The tag type for the replacement of the <paramref name="value"/>.</param>
    /// <param name="preserveValue">If <see langword="true"/> creates a new attribute 'hlt-name' with the name of the replaced (the original) <paramref name="value"/>.</param>
    /// <returns>The current <see cref="TooltipDescription"/> instance.</returns>
    public TooltipDescription AddFontValueReplacement(string value, string replacement, FontTagType fontTagType, bool preserveValue = false)
    {
        ApplyFontValueReplacement(value, replacement, fontTagType, preserveValue);

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

            if (_fontStyleConstantValues is not null)
            {
                _fontStyleConstantValues.Remove(value);
                _fontStyleConstantValues.Add(replacement);
            }
        }
        else if (fontTagType == FontTagType.Style)
        {
            _descriptionParser.AddStyleVarsWithReplacement(value, replacement, preserveValue);

            if (_fontStyleValues is not null)
            {
                _fontStyleValues.Remove(value);
                _fontStyleValues.Add(replacement);
            }
        }
    }

    private void ClearDescriptionsCaches()
    {
        _rawDescription = null;
        _coloredText = null;
        _coloredTextWithScaling = null;
    }
}
