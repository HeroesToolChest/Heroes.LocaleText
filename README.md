# Heroes Locale Text
[![Build and Test](https://github.com/HeroesToolChest/Heroes.LocaleText/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/HeroesToolChest/Heroes.LocaleText/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/release/HeroesToolChest/Heroes.LocaleText.svg)](https://github.com/HeroesToolChest/Heroes.LocaleText/releases/latest) 
[![NuGet](https://img.shields.io/nuget/v/Heroes.LocaleText.svg)](https://www.nuget.org/packages/Heroes.LocaleText/)

Heroes Locale Text is a .NET library that turns Heroes of the Storm gamestrings into readable text. It cleans up the tooltip markup and outputs the text as plain text or with the color and style tags kept, each with or without the per-level scaling info, in any of the 12 game locales.

The gamestrings must already have their data references computed, meaning tags such as `<d ref="..."/>` have been replaced with numbers. The gamestrings from [heroes-data](https://github.com/HeroesToolChest/heroes-data2) are already in this form.

## Gamestring Markup
| Markup | Description |
| --- | --- |
| `<c val="...">text</c>` | Constant tag. The `val` is a hex color (e.g. `bfd4fd`) or a named constant (e.g. `#TooltipNumbers`). |
| `<s val="...">text</s>` | Style tag. The `val` is a named style (e.g. `StandardTooltipHeader`). |
| `<n/>` | Newline. `<n>` and `</n>` are also accepted. |
| `<sp/>` | Space. |
| `~~0.04~~` | Per-level scaling for the value in front of it. `0.04` is 4% per hero level. |
| `##ERROR##` | Error marker, such as one left by a data reference that failed to compute. |

Other tags, such as `<img .../>`, are kept in the outputs that keep tags and removed from the plain text outputs.

## Usage
Create a `GameStringText` from a gamestring, then read one of its properties. Each property is built the first time it is read and then cached.

| Property | Tags | Newlines | Scaling | Error markers |
| --- | :---: | --- | --- | :---: |
| `RawText` | Yes | `<n/>` | `~~0.04~~` | Yes |
| `PlainText` | No | Space | No | No |
| `PlainTextWithNewlines` | No | `<n/>` | No | No |
| `PlainTextWithScaling` | No | Space | `(+4% per level)` | No |
| `PlainTextWithScalingWithNewlines` | No | `<n/>` | `(+4% per level)` | No |
| `ColoredText` | Yes | `<n/>` | No | No |
| `ColoredTextWithScaling` | Yes | `<n/>` | `(+4% per level)` | No |

`<sp/>` tags are kept in `RawText` and become a space in every other output. `ToString()` returns the same as `PlainTextWithScaling`.

```C#
GameStringText gameStringText = new("Spawn a mine that becomes active after a short time. Deals <c val=\"bfd4fd\">153~~0.04~~</c> damage and reveals the enemy for <c val=\"bfd4fd\">4</c> seconds. Lasts <c val=\"bfd4fd\">90</c> seconds.<n/><n/>Stores up to <c val=\"bfd4fd\">3</c> charges.");

string rawText = gameStringText.RawText;
// "Spawn a mine that becomes active after a short time. Deals <c val=\"bfd4fd\">153~~0.04~~</c> damage and reveals the enemy for <c val=\"bfd4fd\">4</c> seconds. Lasts <c val=\"bfd4fd\">90</c> seconds.<n/><n/>Stores up to <c val=\"bfd4fd\">3</c> charges."

string plainText = gameStringText.PlainText;
// "Spawn a mine that becomes active after a short time. Deals 153 damage and reveals the enemy for 4 seconds. Lasts 90 seconds.  Stores up to 3 charges."

string plainTextWithNewlines = gameStringText.PlainTextWithNewlines;
// "Spawn a mine that becomes active after a short time. Deals 153 damage and reveals the enemy for 4 seconds. Lasts 90 seconds.<n/><n/>Stores up to 3 charges."

string plainTextWithScaling = gameStringText.PlainTextWithScaling;
// "Spawn a mine that becomes active after a short time. Deals 153 (+4% per level) damage and reveals the enemy for 4 seconds. Lasts 90 seconds.  Stores up to 3 charges."

string plainTextWithScalingWithNewlines = gameStringText.PlainTextWithScalingWithNewlines;
// "Spawn a mine that becomes active after a short time. Deals 153 (+4% per level) damage and reveals the enemy for 4 seconds. Lasts 90 seconds.<n/><n/>Stores up to 3 charges."

string coloredText = gameStringText.ColoredText;
// "Spawn a mine that becomes active after a short time. Deals <c val=\"bfd4fd\">153</c> damage and reveals the enemy for <c val=\"bfd4fd\">4</c> seconds. Lasts <c val=\"bfd4fd\">90</c> seconds.<n/><n/>Stores up to <c val=\"bfd4fd\">3</c> charges."

string coloredTextWithScaling = gameStringText.ColoredTextWithScaling;
// "Spawn a mine that becomes active after a short time. Deals <c val=\"bfd4fd\">153 (+4% per level)</c> damage and reveals the enemy for <c val=\"bfd4fd\">4</c> seconds. Lasts <c val=\"bfd4fd\">90</c> seconds.<n/><n/>Stores up to <c val=\"bfd4fd\">3</c> charges."
```

### Text Cleanup
Every output also fixes up the markup:
- Nested tags are split into separate, unnested tags.
- Missing end tags are added.
- Tag names are lowercased and extra spaces inside tags are removed.
- `<n>` and `</n>` become `<n/>`.
- Empty tags, such as `<c val="bfd4fd"></c>`, are removed.
- A `%` right after a scaling value is moved in front of it, so `5~~0.04~~%` reads as `5% (+4% per level)`.

```C#
GameStringText gameStringText = new("<C  val=\"FF8000\">Quest:<n>Gain <c val=\"bfd4fd\">5~~0.04~~%</c> Armor");

string rawText = gameStringText.RawText;
// "<c val=\"FF8000\">Quest:</c><n/><c val=\"FF8000\">Gain </c><c val=\"bfd4fd\">5%~~0.04~~</c><c val=\"FF8000\"> Armor</c>"

string coloredTextWithScaling = gameStringText.ColoredTextWithScaling;
// "<c val=\"FF8000\">Quest:</c><n/><c val=\"FF8000\">Gain </c><c val=\"bfd4fd\">5% (+4% per level)</c><c val=\"FF8000\"> Armor</c>"

string plainTextWithScaling = gameStringText.PlainTextWithScaling;
// "Quest: Gain 5% (+4% per level) Armor"
```

## Localization
The second constructor parameter sets the `StormLocale` of the gamestring. The default is `StormLocale.ENUS`. The locale only affects the scaling:
- The scaling value may use a period or the locale's decimal separator, e.g. `~~0.035~~` or `~~0,035~~`.
- The scaling text is written in the locale's language and number format.

```C#
GameStringText gameStringText = new("Feuert drei Geschosse auf ein Zielgebiet, die dem ersten getroffenen Gegner jeweils <c val=\"bfd4fd\">147~~0,035~~</c> Schaden zufügen. Die Geschosse fügen Gebäuden <c val=\"bfd4fd\">50%</c> des Schadens zu.", StormLocale.DEDE);

string result = gameStringText.PlainTextWithScaling;
// result: "Feuert drei Geschosse auf ein Zielgebiet, die dem ersten getroffenen Gegner jeweils 147 (+3,5 % pro Stufe) Schaden zufügen. Die Geschosse fügen Gebäuden 50% des Schadens zu."
```

A scaling value with a comma is only recognized when the locale's decimal separator is a comma. With the default `ENUS`, the `147~~0,035~~` above isn't treated as scaling and is left as-is in every output. If the scaling values only use a period and the scaling text isn't needed, the locale can be left as the default.

| `StormLocale` | Language | Decimal separator | Scaling text for `~~0.035~~` |
| --- | --- | :---: | --- |
| `ENUS` | English | `.` | `(+3.5% per level)` |
| `DEDE` | German | `,` | `(+3,5 % pro Stufe)` |
| `ESES` | Spanish (Spain) | `,` | `(+3,5% por nivel)` |
| `ESMX` | Spanish (Latin America) | `.` | `(+3.5% por nivel)` |
| `FRFR` | French | `,` | `(+3,5 % par niveau)` |
| `ITIT` | Italian | `,` | `(+3,5% per livello)` |
| `KOKR` | Korean | `.` | `(레벨당 +3.5%)` |
| `PLPL` | Polish | `,` | `(+3,5% na poziom)` |
| `PTBR` | Portuguese (Brazil) | `,` | `(+3,5% por nível)` |
| `RURU` | Russian | `,` | `(+3,5% за уровень)` |
| `ZHCN` | Chinese (Simplified) | `.` | `（每级+3.5%）` |
| `ZHTW` | Chinese (Traditional) | `.` | `（每級+3.5%）` |

The scaling text matches the in-game formatting: German and French have a non-breaking space before the `%`, and Chinese uses full-width parentheses with no space in front.

`StormLocaleCulture.GetCultureInfo(StormLocale)` returns the matching `CultureInfo` (e.g. `de-DE` for `DEDE`), for formatting other values the same way.

## Style and Constant Tag Values
The `val` values of the constant tags `<c val="..."/>` and style tags `<s val="..."/>` are called font values.

### Getting the Values
To collect the font values, pass in `true` for the `extractFontValues` parameter. It's off by default since it makes parsing slower. When it's off, `FontConstantValues` and `FontStyleValues` are `null`. Checking `IsFontValuesExtracted` first also tells the compiler they aren't `null`. The values are case-sensitive.

```C#
GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <s val=\"StandardTooltipHeader\">2.75</s> seconds.", extractFontValues: true);

if (gameStringText.IsFontValuesExtracted)
{
    IReadOnlySet<string> constantValues = gameStringText.FontConstantValues;
    IReadOnlySet<string> styleValues = gameStringText.FontStyleValues;

    // constantValues: { "#TooltipNumbers" }
    // styleValues: { "StandardTooltipHeader" }
}
```

### Replacing the Values
Font values can be swapped for new values, e.g. to turn named constants such as `#TooltipNumbers` into hex colors for displaying the text outside of the game. There are four methods:
- `AddFontValueReplacement(value, replacement, fontTagType, preserveValue = false)` for a single value.
- `AddFontValueReplacements(fontTagType, preserveValues, newValuesByValue)` with an `IDictionary<string, string>`, an `IEnumerable<(string Value, string Replacement)>`, or the tuples passed in directly.

How replacements work:
- `FontTagType.Constant` replaces the values of `<c>` tags and `FontTagType.Style` replaces the values of `<s>` tags.
- Values are matched case-sensitively.
- The replacements show up in `RawText`, `ColoredText`, and `ColoredTextWithScaling`. The plain text outputs have no tags, so they aren't affected.
- If the font values were extracted, `FontConstantValues` and `FontStyleValues` list the new values instead of the replaced ones.
- `extractFontValues` doesn't need to be set.
- Each method returns the same `GameStringText`, so calls can be chained. Replacements can be added at any time, and the affected outputs are rebuilt the next time they're read.

```C#
GameStringText gameStringText = new GameStringText("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <s val=\"StandardTooltipHeader\">2.75</s> seconds.")
    .AddFontValueReplacement("#TooltipNumbers", "bfd4fd", FontTagType.Constant)
    .AddFontValueReplacement("StandardTooltipHeader", "ffffff", FontTagType.Style);

string result = gameStringText.ColoredText;
// result: "Every <c val=\"bfd4fd\">18</c> seconds, deals <c val=\"bfd4fd\">125</c><n/> extra damage every <s val=\"ffffff\">2.75</s> seconds."
```

The same replacements using the other overloads:
```C#
Dictionary<string, string> constantReplacements = new()
{
    ["#TooltipNumbers"] = "bfd4fd",
};

gameStringText.AddFontValueReplacements(FontTagType.Constant, false, constantReplacements);
gameStringText.AddFontValueReplacements(FontTagType.Style, false, ("StandardTooltipHeader", "ffffff"));
```

### Keeping the Original Values
To keep the original value while replacing it, set the `preserveValues` parameter to `true`. This adds an `hlt-name` attribute that holds the original value. A tag that already has an `hlt-name` attribute keeps its existing one.

```C#
GameStringText gameStringText = new GameStringText("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>")
    .AddFontValueReplacements(FontTagType.Style, true, ("StandardTooltipHeader", "123456"), ("StandardTooltipDetails2", "222222"));

string result = gameStringText.ColoredText;
// result: "<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"222222\" hlt-name=\"StandardTooltipDetails2\">Cooldown: </s>"
```

### Removing the 'hlt-name' Attribute
To remove existing `hlt-name` attributes, call `RemoveHltNameForStyleTags` or `RemoveHltNameForConstantTags`. Each takes an `undoFontValueReplacement` parameter:
- `false` removes the attribute and leaves the `val` as is.
- `true` removes the attribute and sets the `val` back to the removed `hlt-name` value, undoing the earlier replacement.

Other attributes on the tag are kept, and the `hlt-name` attribute can be in any position. Only the `hlt-name` attributes that are already in the gamestring are removed, not the ones added by a replacement on the same `GameStringText`.

```C#
// undoFontValueReplacement is false
GameStringText gameStringText = new GameStringText("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\" otherAtt=\"somevalue\">Archon</s> more text <c val=\"123456\" hlt-name=\"#TooltipNumbers\" otherAtt2=\"somevalue2\">18</c> seconds")
    .RemoveHltNameForStyleTags(false)
    .RemoveHltNameForConstantTags(false);

string result = gameStringText.RawText;
// the hlt-name attributes are removed
// result: "<s val=\"123456\" otherAtt=\"somevalue\">Archon</s> more text <c val=\"123456\" otherAtt2=\"somevalue2\">18</c> seconds"
```
```C#
// undoFontValueReplacement is true
GameStringText gameStringText = new GameStringText("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\" otherAtt=\"somevalue\">Archon</s> more text <c val=\"123456\" hlt-name=\"#TooltipNumbers\" otherAtt2=\"somevalue2\">18</c> seconds")
    .RemoveHltNameForStyleTags(true)
    .RemoveHltNameForConstantTags(true);

string result = gameStringText.RawText;
// the hlt-name attributes are removed and each val is set back to its hlt-name value
// result: "<s val=\"StandardTooltipHeader\" otherAtt=\"somevalue\">Archon</s> more text <c val=\"#TooltipNumbers\" otherAtt2=\"somevalue2\">18</c> seconds"
```

The removal always happens before any replacements, no matter the order of the calls. This allows a gamestring saved with preserved values to be mapped to a different set of values:
```C#
GameStringText gameStringText = new GameStringText("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon</s>")
    .AddFontValueReplacement("StandardTooltipHeader", "ffffff", FontTagType.Style)
    .RemoveHltNameForStyleTags(true);

string result = gameStringText.ColoredText;
// result: "<s val=\"ffffff\">Archon</s>"
```

## Developing
To build and compile the code, it is recommended to use the latest version of [Visual Studio 2026 or Visual Studio Code](https://visualstudio.microsoft.com/downloads/).

Another option is to use the dotnet CLI tools from the latest [.NET SDK](https://dotnet.microsoft.com/download).

## License
[MIT license](https://github.com/HeroesToolChest/Heroes.LocaleText/blob/main/LICENSE)
