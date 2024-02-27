namespace Heroes.LocaleText;

[Flags]
internal enum TagFlag : byte
{
    None = 0,
    Include = 1 << 0,
    Eval = 1 << 1,
}