using System.Globalization;

namespace Heroes.LocaleText;

/// <summary>
/// Class for obtaining the heroes localization data.
/// </summary>
public static class StormLocaleCulture
{
    /// <summary>
    /// Gets the <see cref="CultureInfo"/> for the <paramref name="stormLocale"/>.
    /// </summary>
    /// <param name="stormLocale">The locale.</param>
    /// <returns>A cached, read-only <see cref="CultureInfo"/>.</returns>
    public static CultureInfo GetCultureInfo(StormLocale stormLocale) => CultureInfo.GetCultureInfo(GetCultureName(stormLocale));

    private static string GetCultureName(StormLocale stormLocale) => stormLocale switch
    {
        StormLocale.ENUS => "en-US",
        StormLocale.DEDE => "de-DE",
        StormLocale.ESES => "es-ES",
        StormLocale.ESMX => "es-MX",
        StormLocale.FRFR => "fr-FR",
        StormLocale.ITIT => "it-IT",
        StormLocale.KOKR => "ko-KR",
        StormLocale.PLPL => "pl-PL",
        StormLocale.PTBR => "pt-BR",
        StormLocale.RURU => "ru-RU",
        StormLocale.ZHCN => "zh-CN",
        StormLocale.ZHTW => "zh-TW",

        _ => "en-US",
    };
}
