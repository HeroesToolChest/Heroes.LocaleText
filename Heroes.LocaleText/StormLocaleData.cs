using System.Globalization;

namespace Heroes.LocaleText;

/// <summary>
/// Class for obtaining the heroes localization data.
/// </summary>
public static class StormLocaleData
{
    /// <summary>
    /// Gets the <see cref="CultureInfo"/>.
    /// </summary>
    /// <param name="stormLocale">The locale.</param>
    /// <returns>The <see cref="CultureInfo"/>.</returns>
    public static CultureInfo GetCultureInfo(StormLocale stormLocale) => stormLocale switch
    {
        StormLocale.ENUS => new CultureInfo("en-US"),
        StormLocale.DEDE => new CultureInfo("de-DE"),
        StormLocale.ESES => new CultureInfo("es-ES"),
        StormLocale.ESMX => new CultureInfo("es-MX"),
        StormLocale.FRFR => new CultureInfo("fr-FR"),
        StormLocale.ITIT => new CultureInfo("it-IT"),
        StormLocale.KOKR => new CultureInfo("ko-KR"),
        StormLocale.PLPL => new CultureInfo("pl-PL"),
        StormLocale.PTBR => new CultureInfo("pt-BR"),
        StormLocale.RURU => new CultureInfo("ru-RU"),
        StormLocale.ZHCN => new CultureInfo("zh-CN"),
        StormLocale.ZHTW => new CultureInfo("zh-TW"),

        _ => new CultureInfo("en-US"),
    };
}

