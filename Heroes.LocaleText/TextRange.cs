namespace Heroes.LocaleText;

// stores offsets instead of a Range, so slicing skips the from-end handling and the extra bounds check
internal readonly struct TextRange(int start, int end, TextType type)
{
    public TextRange(Range range, TextType type)
        : this(range.Start.Value, range.End.Value, type)
    {
    }

    public int Start { get; } = start;

    public int Length { get; } = end - start;

    public TextType Type { get; } = type;

    public ReadOnlySpan<char> GetText(ReadOnlySpan<char> gameString) => gameString.Slice(Start, Length);
}
