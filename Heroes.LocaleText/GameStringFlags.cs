namespace Heroes.LocaleText;

internal readonly struct GameStringFlags
{
    public TagFlag ColorTags { get; init; }

    public TagFlag ScalingTag { get; init; }

    public TagFlag NewLineTag { get; init; }

    public TagFlag ErrorTag { get; init; }

    public TagFlag SpaceTag { get; init; }
}
