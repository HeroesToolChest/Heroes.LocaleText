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
    /// <param name="text">A parsed description that has not been modified into a readable verbiage (e.g. PlainText or ColorText from this class should not be used).</param>
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
    /// <returns>A collection of text style values.</returns>
    public IEnumerable<string>? FontStyleValues
    {
        get
        {
            if (_rawDescription is null)
                _ = RawDescription; // trigger the parsing

            return _descriptionParser.StyleTagVariables;
        }
    }

    /// <summary>
    /// <para>Gets a collection of text style constant values used in the tooltip description.</para>
    /// <para>
    /// Example:<br/>
    /// With &lt;c val=\"#TooltipNumbers\"&gt;&lt;/c&gt; returns #TooltipNumbers.
    /// </para>
    /// </summary>
    /// <returns>A collection of text style constant values.</returns>
    public IEnumerable<string>? FontStyleConstantValues
    {
        get
        {
            if (_rawDescription is null)
                _ = RawDescription; // trigger the parsing

            return _descriptionParser.StyleConstantTagVariables;
        }
    }

    /// <summary>
    /// Adds a dictionary of values that will be replaced by new values.
    /// </summary>
    /// <param name="newValuesByValue">A dictionary of values and their replacement values. Is case-sensitive.</param>
    /// <param name="fontTagType">The tag type for the replacement of the values.</param>
    /// <returns>The current <see cref="TooltipDescription"/> instance.</returns>
    public TooltipDescription AddFontValueReplacements(IDictionary<string, string> newValuesByValue, FontTagType fontTagType)
    {
        foreach (var item in newValuesByValue)
        {
            AddFontValueReplacements(item.Key, item.Value, fontTagType);
        }

        return this;
    }

    /// <summary>
    /// Adds a collection of values that will be replaced by new values.
    /// </summary>
    /// <param name="newValuesByValue">A collection of values and their replacement values. Is case-sensitive.</param>
    /// <param name="fontTagType">The tag type for the replacement of the values.</param>
    /// <returns>The current <see cref="TooltipDescription"/> instance.</returns>
    public TooltipDescription AddFontValueReplacements(IEnumerable<(string Value, string Replacement)> newValuesByValue, FontTagType fontTagType)
    {
        foreach ((string value, string replacement) in newValuesByValue)
        {
            AddFontValueReplacements(value, replacement, fontTagType);
        }

        return this;
    }

    /// <summary>
    /// Adds a value that will be replaced by a new value.
    /// </summary>
    /// <param name="value">The value of the val attribute of the tag. Is case-sensitive.</param>
    /// <param name="replacement">The new value that will replace <paramref name="value"/>. Is case-sensitive.</param>
    /// <param name="fontTagType">The tag type for the replacement of the <paramref name="value"/>.</param>
    /// <returns>The current <see cref="TooltipDescription"/> instance.</returns>
    public TooltipDescription AddFontValueReplacements(string value, string replacement, FontTagType fontTagType)
    {
        if (fontTagType == FontTagType.Constant)
            _descriptionParser.AddStyleConstantVarsWithReplacement(value, replacement);
        else if (fontTagType == FontTagType.Style)
            _descriptionParser.AddStyleVarsWithReplacement(value, replacement);

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
}
