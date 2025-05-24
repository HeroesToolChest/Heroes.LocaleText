# Heroes Locale Text
[![Build and Test](https://github.com/HeroesToolChest/Heroes.LocaleText/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/HeroesToolChest/Heroes.LocaleText/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/release/HeroesToolChest/Heroes.LocaleText.svg)](https://github.com/HeroesToolChest/Heroes.LocaleText/releases/latest) 
[![NuGet](https://img.shields.io/nuget/v/Heroes.LocaleText.svg)](https://www.nuget.org/packages/Heroes.LocaleText/)

Heroes Locale Text is a .NET library used to parse tooltip descriptions and provide readable friendly verbiage. Descriptions must already be xml parsed out and it's values computed. 

Valid descriptions are the gamestrings from [heroes-data](https://github.com/HeroesToolChest/heroes-data).

## Usage
Parse a provided gamestring using `TooltipDescription` and call one of the properties.
```C#
TooltipDescription tooltipDescription = new("Spawn a mine that becomes active after a short time. Deals <c val=\"bfd4fd\">153~~0.04~~</c> damage and reveals the enemy for <c val=\"bfd4fd\">4</c> seconds. Lasts <c val=\"bfd4fd\">90</c> seconds.<n/><n/>Stores up to <c val=\"bfd4fd\">3</c> charges.");

string a = tooltipDescription.RawDescription;
string b = tooltipDescription.PlainText;
string d = tooltipDescription.PlainTextWithNewlines;
string c = tooltipDescription.PlainTextWithScaling;
string e = tooltipDescription.PlainTextWithScalingWithNewlines;
string f = tooltipDescription.ColoredText;
string g = tooltipDescription.ColoredTextWithScaling;

// a: "Spawn a mine that becomes active after a short time. Deals <c val=\"bfd4fd\">153~~0.04~~</c> damage and reveals the enemy for <c val=\"bfd4fd\">4</c> seconds. Lasts <c val=\"bfd4fd\">90</c> seconds.<n/><n/>Stores up to <c val=\"bfd4fd\">3</c> charges."
// b: "Spawn a mine that becomes active after a short time. Deals 153 damage and reveals the enemy for 4 seconds. Lasts 90 seconds.  Stores up to 3 charges."
// c: "Spawn a mine that becomes active after a short time. Deals 153 damage and reveals the enemy for 4 seconds. Lasts 90 seconds.<n/><n/>Stores up to 3 charges."
// d: "Spawn a mine that becomes active after a short time. Deals 153 (+4% per level) damage and reveals the enemy for 4 seconds. Lasts 90 seconds.  Stores up to 3 charges."
// e: "Spawn a mine that becomes active after a short time. Deals 153 (+4% per level) damage and reveals the enemy for 4 seconds. Lasts 90 seconds.<n/><n/>Stores up to 3 charges."
// f: "Spawn a mine that becomes active after a short time. Deals <c val=\"bfd4fd\">153</c> damage and reveals the enemy for <c val=\"bfd4fd\">4</c> seconds. Lasts <c val=\"bfd4fd\">90</c> seconds.<n/><n/>Stores up to <c val=\"bfd4fd\">3</c> charges."
// g: "Spawn a mine that becomes active after a short time. Deals <c val=\"bfd4fd\">153 (+4% per level)</c> damage and reveals the enemy for <c val=\"bfd4fd\">4</c> seconds. Lasts <c val=\"bfd4fd\">90</c> seconds.<n/><n/>Stores up to <c val=\"bfd4fd\">3</c> charges."
```

### Localization
The localization of the gamestring can also be passed in using `StormLocale`. By default it is ENUS. The localization is used only for the scaling text (e.g. (+4% per level)). If the scaling text is not needed, then the `StormLocale` may be left as the default.

```C#
TooltipDescription tooltipDescription = new("Feuert drei Geschosse auf ein Zielgebiet, die dem ersten getroffenen Gegner jeweils <c val=\"bfd4fd\">147~~0.035~~</c> Schaden zufügen. Die Geschosse fügen Gebäuden <c val=\"bfd4fd\">50%</c> des Schadens zu.", StormLocale.DEDE);

string result = tooltipDescription.PlainTextWithScaling;
// result: "Feuert drei Geschosse auf ein Zielgebiet, die dem ersten getroffenen Gegner jeweils 147 (+3,5% pro Stufe) Schaden zufügen. Die Geschosse fügen Gebäuden 50% des Schadens zu."
```

### Font Styles
To get all the values of the constant tags `<c val=""/>` and style tags `<s val=""/>`, pass in `true` to the `extractFontValues` parameter.
```C#
TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <s val=\"StandardTooltipHeader\">2.75</s> seconds.", extractFontValues: true);

if (tooltipDescription.IsFontValuesExtracted)
{
    IEnumerable<string>? constantValues = tooltipDescription.FontStyleConstantValues;
    IEnumerable<string>? styleValues = tooltipDescription.FontStyleValues;

    // constantValues: { "#TooltipNumbers" }
    // styleValues: { "StandardTooltipHeader" }
}
```

To replace the values of the constant tags and style tags, use the `AddFontValueReplacements` methods. Specify the key, the `val` that should be replaced, and the value which is the new value.

```C#
// note: extractFontValues does not need to be set
TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <s val=\"StandardTooltipHeader\">2.75</s> seconds.");

Dictionary<string, string> constantKeyValuePairs = [];
constantKeyValuePairs.Add("#TooltipNumbers", "123456");

tooltipDescription.AddFontValueReplacements(FontTagType.Constant, false, constantKeyValuePairs);

Dictionary<string, string> styleKeyValuePairs = [];
styleKeyValuePairs.Add("StandardTooltipHeader", "789012");

tooltipDescription.AddFontValueReplacements(FontTagType.Style, false, styleKeyValuePairs);

string result = tooltipDescription.ColoredText;

// result: "Every <c val="123456">18</c> seconds, deals <c val="123456">125</c><n/> extra damage every <s val="789012">2.75</s> seconds."
```

To keep the original value of the constant tags or style tags while replacing the value, set the `preserveValues` parameter to `true`. This will create a new attribute `hlt-name` with the original value.

```C#
TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>");

Dictionary<string, string> keyValuePairs = [];
keyValuePairs.Add("StandardTooltipHeader", "123456");
keyValuePairs.Add("StandardTooltipDetails2", "222222");

tooltipDescription.AddFontValueReplacements(FontTagType.Style, true, keyValuePairs);

string result = tooltipDescription.ColoredText;

// result: "<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"222222\" hlt-name=\"StandardTooltipDetails2\">Cooldown: </s>"
```

## Developing
To build and compile the code, it is recommended to use the latest version of [Visual Studio 2022 or Visual Studio Code](https://visualstudio.microsoft.com/downloads/).

Another option is to use the dotnet CLI tools from the latest [.NET SDK](https://dotnet.microsoft.com/download).

## License
[MIT license](/LICENSE)