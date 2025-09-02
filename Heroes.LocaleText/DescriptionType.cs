namespace Heroes.LocaleText;

/// <summary>
/// Specifies the gamestring type.
/// </summary>
public enum DescriptionType
{
    /// <summary>
    /// Indicates raw description.
    /// </summary>
    RawDescription = 0,

    /// <summary>
    /// Indicates plain text.
    /// </summary>
    PlainText = 1,

    /// <summary>
    /// Indicates plain text with new lines.
    /// </summary>
    PlainTextWithNewlines = 2,

    /// <summary>
    /// Indicates plain text with scaling.
    /// </summary>
    PlainTextWithScaling = 3,

    /// <summary>
    /// Indicates plain text with scaling with new lines.
    /// </summary>
    PlainTextWithScalingWithNewlines = 4,

    /// <summary>
    /// Indicates colored text.
    /// </summary>
    ColoredText = 5,

    /// <summary>
    /// Indicates colored text with scaling.
    /// </summary>
    ColoredTextWithScaling = 6,
}
