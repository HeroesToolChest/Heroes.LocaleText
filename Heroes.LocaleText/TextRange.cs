namespace Heroes.LocaleText;

internal readonly struct TextRange(Range range, TextType type)
{
    public Range Range { get; } = range;

    public TextType Type { get; } = type;
}
