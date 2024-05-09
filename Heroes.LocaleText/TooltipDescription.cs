﻿using System.Diagnostics;

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
    /// <param name="extractStyleVars">If <see langword="true"/>, then the property <see cref="TextStyleVariables"/> may have items.</param>
    public TooltipDescription(string text, StormLocale gameStringLocale = StormLocale.ENUS, bool extractStyleVars = false)
    {
        ArgumentNullException.ThrowIfNull(text);

        GameStringLocale = gameStringLocale;
        IsStyleVarsExtracted = extractStyleVars;

        _descriptionParser = DescriptionParser.GetInstance(text, gameStringLocale, extractStyleVars);
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
    /// Gets a value indicating whether the style vars have been extracted.
    /// </summary>
    public bool IsStyleVarsExtracted { get; }

    /// <summary>
    /// Gets a collection of text style variables used in the tooltip description.
    /// </summary>
    /// <returns>A collection of text style variables.</returns>
    public IEnumerable<string> TextStyleVariables
    {
        get
        {
            if (_rawDescription is null)
                _ = RawDescription; // trigger the parsing

            return _descriptionParser.StyleTagVariables;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return PlainTextWithScaling;
    }
}
