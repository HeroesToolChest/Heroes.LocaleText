﻿using Newtonsoft.Json.Linq;

namespace Heroes.LocaleText.Tests;

[TestClass]
public class TooltipDescriptionTests
{
    private readonly string _testText = "Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.";
    private readonly string _testTextDeDE = "Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern <c val=\"bfd4fd\">70~~0.045~~</c> Schaden zu, verlangsamt sie um <c val=\"bfd4fd\">35%</c> und blendet sie <c val=\"bfd4fd\">1,75</c> Sek. lang.";

    [TestMethod]
    public void RawDescription_NullText_ThrowsException()
    {
        // arrange

        // act
        Action act = () => new TooltipDescription(null!);

        // assert
        Assert.ThrowsException<ArgumentNullException>(act);
    }

    [TestMethod]
    public void RawDescription_EmptyText_ReturnsEmptyDescription()
    {
        // arrange
        TooltipDescription tooltipDescription = new(string.Empty);

        // act
        string result = tooltipDescription.RawDescription;

        // assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RawDescription_TestText_ReturnsRawDescription()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // act
        string result = tooltipDescription.RawDescription;

        // assert
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void PlainText_TestText_ReturnsPlainText()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // act
        string result = tooltipDescription.PlainText;

        // assert
        Assert.AreEqual("Every 18 seconds, deals 125  extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithNewlines_TestText_ReturnsPlainTextWithNewlines()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // act
        string result = tooltipDescription.PlainTextWithNewlines;

        // assert
        Assert.AreEqual("Every 18 seconds, deals 125<n/> extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithScaling_TestText_ReturnsPlainTextWithScaling()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // act
        string result = tooltipDescription.PlainTextWithScaling;

        // assert
        Assert.AreEqual("Every 18 seconds, deals 125 (+4.5% per level)  extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithScalingWithNewlines_TestText_ReturnsPlainTextWithScalingWithNewlines()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // act
        string result = tooltipDescription.PlainTextWithScalingWithNewlines;

        // assert
        Assert.AreEqual("Every 18 seconds, deals 125 (+4.5% per level)<n/> extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void ColoredText_TestText_ReturnsColoredText()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void ColoredTextWithScaling_TestText_ReturnsColoredTextWithScaling()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // act
        string result = tooltipDescription.ColoredTextWithScaling;

        // assert
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125 (+4.5% per level)</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void ColoredTextWithScaling_TestTextDeDe_ReturnsColoredTextWithScaling()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testTextDeDE, StormLocale.DEDE);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern <c val=\"bfd4fd\">70</c> Schaden zu, verlangsamt sie um <c val=\"bfd4fd\">35%</c> und blendet sie <c val=\"bfd4fd\">1,75</c> Sek. lang.", result);
    }

    [TestMethod]
    public void ToString_TestText_ReturnsPlainTextWithScaling()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);
        string plainTextWithScaling = tooltipDescription.PlainTextWithScaling;

        // act
        string result = tooltipDescription.ToString();

        // assert
        Assert.AreEqual(plainTextWithScaling, result);
    }

    [TestMethod]
    public void GameStringLocale_TestTextDefault_ReturnsStormLocale()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // act
        StormLocale result = tooltipDescription.GameStringLocale;

        // assert
        Assert.AreEqual(StormLocale.ENUS, result);
    }

    [TestMethod]
    public void GameStringLocale_TestTextDeDe_ReturnsStormLocale()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText, StormLocale.DEDE);

        // act
        StormLocale result = tooltipDescription.GameStringLocale;

        // assert
        Assert.AreEqual(StormLocale.DEDE, result);
    }

    [TestMethod]
    public void FontStyleVariables_HasStyleVariables_ReturnsVars()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s>", extractFontValues: true);

        // act
        IEnumerable<string>? result = tooltipDescription.FontStyleValues;

        // assert
        CollectionAssert.AreEqual(
            new List<string>()
            {
                "StandardTooltipHeader",
            },
            result!.ToList());
    }

    [TestMethod]
    public void FontStyleConstantVariables_HasStyleConstantVariables_ReturnsVars()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds.", extractFontValues: true);

        // act
        IEnumerable<string>? result = tooltipDescription.FontStyleConstantValues;

        // assert
        CollectionAssert.AreEqual(
            new List<string>()
            {
                "#TooltipNumbers",
            },
            result!.ToList());
    }

    [TestMethod]
    public void FontStyleVariables_DoesNotHaveStyleVariables_ReturnsEmpty()
    {
        // arrange
        TooltipDescription tooltipDescription = new("test text", extractFontValues: true);

        // act
        IEnumerable<string>? result = tooltipDescription.FontStyleValues;

        // assert
        Assert.IsTrue(tooltipDescription.IsFontValuesExtracted);
        CollectionAssert.AreEqual(
            new List<string>()
            {
            },
            result!.ToList());
    }

    [TestMethod]
    public void FontConstantStyleVariables_DoesNotHaveStyleConstantVariables_ReturnsEmpty()
    {
        // arrange
        TooltipDescription tooltipDescription = new("test text", extractFontValues: true);

        // act
        IEnumerable<string>? result = tooltipDescription.FontStyleConstantValues;

        // assert
        Assert.IsTrue(tooltipDescription.IsFontValuesExtracted);
        CollectionAssert.AreEqual(
            new List<string>()
            {
            },
            result!.ToList());
    }

    [TestMethod]
    public void FontStyleVariables_ExtractSetToFalse_ReturnsEmpty()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText, extractFontValues: false);

        // act
        IEnumerable<string>? result = tooltipDescription.FontStyleValues;

        // assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FontStyleConstantVariables_ExtractSetToFalse_ReturnsEmpty()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText, extractFontValues: false);

        // act
        IEnumerable<string>? result = tooltipDescription.FontStyleConstantValues;

        // assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithDictionaryConstant_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        Dictionary<string, string> keyValuePairs = [];
        keyValuePairs.Add("#TooltipNumbers", "123456");
        keyValuePairs.Add("TooltipNumbers", "123456");
        keyValuePairs.Add("TooltipNumbers2", "222222");
        keyValuePairs.Add("TooltipNumbers3", "333333");

        tooltipDescription.AddFontValueReplacements(FontTagType.Constant, false, keyValuePairs);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\">18</c> seconds, deals <c val=\"123456\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithDictionaryStyle_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        Dictionary<string, string> keyValuePairs = [];
        keyValuePairs.Add("StandardTooltipHeader", "123456");
        keyValuePairs.Add("StandardTooltipDetails2", "222222");

        tooltipDescription.AddFontValueReplacements(FontTagType.Style, false, keyValuePairs);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\">Archon </s><n/><s val=\"222222\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithDictionaryConstantPreserve_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        Dictionary<string, string> keyValuePairs = [];
        keyValuePairs.Add("#TooltipNumbers", "123456");
        keyValuePairs.Add("TooltipNumbers", "123456");
        keyValuePairs.Add("TooltipNumbers2", "222222");
        keyValuePairs.Add("TooltipNumbers3", "333333");

        tooltipDescription.AddFontValueReplacements(FontTagType.Constant, true, keyValuePairs);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\" hlt-name=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"123456\" hlt-name=\"TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithDictionaryStylePreserve_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        Dictionary<string, string> keyValuePairs = [];
        keyValuePairs.Add("StandardTooltipHeader", "123456");
        keyValuePairs.Add("StandardTooltipDetails2", "222222");

        tooltipDescription.AddFontValueReplacements(FontTagType.Style, true, keyValuePairs);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"222222\" hlt-name=\"StandardTooltipDetails2\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithTupleConstant_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("#TooltipNumbers", "123456"));
        values.Add(("TooltipNumbers2", "123456"));
        values.Add(("TooltipNumbers3", "123456"));

        tooltipDescription.AddFontValueReplacements(FontTagType.Constant, false, values);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\">18</c> seconds, deals <c val=\"123456\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithTupleStyle_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("StandardTooltipHeader", "123456"));
        values.Add(("StandardTooltipDetails2", "222222"));

        tooltipDescription.AddFontValueReplacements(FontTagType.Style, false, values);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\">Archon </s><n/><s val=\"222222\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithTupleConstantPreserve_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("#TooltipNumbers", "123456"));
        values.Add(("TooltipNumbers2", "123456"));
        values.Add(("TooltipNumbers3", "123456"));

        tooltipDescription.AddFontValueReplacements(FontTagType.Constant, true, values);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\" hlt-name=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"123456\" hlt-name=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithTupleStylePreserve_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("StandardTooltipHeader", "123456"));
        values.Add(("StandardTooltipDetails2", "222222"));

        tooltipDescription.AddFontValueReplacements(FontTagType.Style, true, values);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"222222\" hlt-name=\"StandardTooltipDetails2\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_SingleConstant_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new TooltipDescription("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false)
            .AddFontValueReplacement("#TooltipNumbers", "123456", FontTagType.Constant);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\">18</c> seconds, deals <c val=\"123456\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_SingleConstantPreserve_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new TooltipDescription("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false)
            .AddFontValueReplacement("#TooltipNumbers", "123456", FontTagType.Constant, preserveValue: true);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\" hlt-name=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"123456\" hlt-name=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_SingleStyle_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new TooltipDescription("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipHeader\">Cooldown: </s>", extractFontValues: false)
            .AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\">Archon </s><n/><s val=\"123456\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_SingleStylePreserve_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new TooltipDescription("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipHeader\">Cooldown: </s>", extractFontValues: false)
            .AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Cooldown: </s>", result);
    }

#if NET9_0_OR_GREATER
    [TestMethod]
    public void AddFontVarReplacements_WithParamsConstant_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        tooltipDescription.AddFontValueReplacements(FontTagType.Constant, false, ("#TooltipNumbers", "123456"), ("TooltipNumbers2", "123456"), ("TooltipNumbers3", "123456"));

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\">18</c> seconds, deals <c val=\"123456\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithParamsStyle_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        tooltipDescription.AddFontValueReplacements(FontTagType.Style, false, ("StandardTooltipHeader", "123456"), ("StandardTooltipDetails2", "222222"));

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\">Archon </s><n/><s val=\"222222\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithParamsConstantPreserve_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("#TooltipNumbers", "123456"));
        values.Add(("TooltipNumbers2", "123456"));
        values.Add(("TooltipNumbers3", "123456"));

        tooltipDescription.AddFontValueReplacements(FontTagType.Constant, preserveValues: true, values);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\" hlt-name=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"123456\" hlt-name=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithParamsStylePreserve_ReturnsResultWithReplace()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("StandardTooltipHeader", "123456"));
        values.Add(("StandardTooltipDetails2", "222222"));

        tooltipDescription.AddFontValueReplacements(FontTagType.Style, preserveValues: true, values);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"222222\" hlt-name=\"StandardTooltipDetails2\">Cooldown: </s>", result);
    }
#endif

    [TestMethod]
    public void AddFontVarReplacement_ExtractFontValuesThenAddReplacements_ColorDescriptionsShouldBeUpdated()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: true);
        if (tooltipDescription.IsFontValuesExtracted)
        {
            // extract
            _ = tooltipDescription.FontStyleValues.ToList();
        }

        // then add
        tooltipDescription.AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);

        // act
        string resultRaw = tooltipDescription.RawDescription;
        string resultPlainText = tooltipDescription.PlainText;
        string resultPlainTextWithScaling = tooltipDescription.PlainTextWithScaling;
        string resultPlainTextWithNewlines = tooltipDescription.PlainTextWithNewlines;
        string resultPlainTextWithScalingWithNewlines = tooltipDescription.PlainTextWithScalingWithNewlines;
        string resultColorText = tooltipDescription.ColoredText;
        string resultColoredTextWithScaling = tooltipDescription.ColoredTextWithScaling;

        // assert
        Assert.IsTrue(tooltipDescription.IsFontValuesExtracted);
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", resultRaw);
        Assert.AreEqual("Archon  Cooldown: ", resultPlainText);
        Assert.AreEqual("Archon  Cooldown: ", resultPlainTextWithScaling);
        Assert.AreEqual("Archon <n/>Cooldown: ", resultPlainTextWithNewlines);
        Assert.AreEqual("Archon <n/>Cooldown: ", resultPlainTextWithScalingWithNewlines);
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", resultColorText);
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", resultColoredTextWithScaling);
    }

    [TestMethod]
    public void AddFontVarReplacement_GetColoredTextThenAddReplacements_ColorTextShouldBeUpdated()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);
        string originalColorText = tooltipDescription.ColoredText;

        // then add
        tooltipDescription.AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);

        // act
        string result = tooltipDescription.ColoredText;

        // assert
        Assert.AreNotEqual(originalColorText, result);
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacement_PlainTextThenAddReplacements_NoChanges()
    {
        // arrange
        TooltipDescription tooltipDescription = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);
        string originalPlainText = tooltipDescription.PlainText;
        string originalPlaintTextWithScaling = tooltipDescription.PlainTextWithScaling;
        string originalPlainTextWithNewlines = tooltipDescription.PlainTextWithNewlines;
        string originalPlainTextWithScalingWithNewlines = tooltipDescription.PlainTextWithScalingWithNewlines;

        // then add
        tooltipDescription.AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);

        // act
        string result = tooltipDescription.PlainText;
        string resultPlainTextWithScaling = tooltipDescription.PlainTextWithScaling;
        string resultPlainTextWithNewlines = tooltipDescription.PlainTextWithNewlines;
        string resultPlainTextWithScalingWithNewlines = tooltipDescription.PlainTextWithScalingWithNewlines;

        // assert
        Assert.AreEqual(originalPlainText, result);
        Assert.AreEqual(originalPlaintTextWithScaling, resultPlainTextWithScaling);
        Assert.AreEqual(originalPlainTextWithNewlines, resultPlainTextWithNewlines);
        Assert.AreEqual(originalPlainTextWithScalingWithNewlines, resultPlainTextWithScalingWithNewlines);
    }

    [TestMethod]
    public void FontStyleValues_AddedFontValueReplacement_UpdatedFontStyleValues()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, <s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: true);

        List<string> originalFontStyleValues = [.. tooltipDescription.FontStyleValues!];
        List<string> originalFontStyleConstantValues = [.. tooltipDescription.FontStyleConstantValues!];

        // then add
        tooltipDescription.AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);
        tooltipDescription.AddFontValueReplacement("#TooltipNumbers", "aaaaaaaa", FontTagType.Constant, preserveValue: true);
        _ = tooltipDescription.RawDescription;

        // act
        List<string> fontStyleValues = [.. tooltipDescription.FontStyleValues!];
        List<string> fontStyleConstantValues = [.. tooltipDescription.FontStyleConstantValues!];

        // assert
        Assert.HasCount(2, originalFontStyleValues);
        Assert.Contains("StandardTooltipHeader", originalFontStyleValues);
        Assert.Contains("StandardTooltipDetails2", originalFontStyleValues);
        Assert.HasCount(1, originalFontStyleConstantValues);
        Assert.Contains("#TooltipNumbers", originalFontStyleConstantValues);

        Assert.HasCount(2, fontStyleValues);
        Assert.DoesNotContain("StandardTooltipHeader", fontStyleValues);
        Assert.Contains("StandardTooltipDetails2", fontStyleValues);
        Assert.Contains("123456", fontStyleValues);
        Assert.HasCount(1, fontStyleConstantValues);
        Assert.Contains("aaaaaaaa", fontStyleConstantValues);
    }

    [TestMethod]
    public void FontStyleValues_FrenchQuoteAtEndOfCVal_ReturnsFontStyleValue()
    {
        // arrange
        TooltipDescription tooltipDescription = new("Attaquer un héros ralenti, immobilisé ou étourdi augmente les dégâts des attaques de base de Grisetête de <c val=\"#TooltipNumbers\">0 %</c> pendant <c val=\"#TooltipNumbers\">0</c> secondes. Ce bonus passe à <c val=\"#TooltipNumbers\">0 %</c> en forme de <c val=\"#ColorViolet »>worgen</c>.", gameStringLocale: StormLocale.DEDE, extractFontValues: true);

        // act
        List<string> originalFontStyleConstantValues = [.. tooltipDescription.FontStyleConstantValues!];

        // assert
        Assert.HasCount(1, originalFontStyleConstantValues); // only one, the other is broken by the french quote
        Assert.Contains("#TooltipNumbers", originalFontStyleConstantValues);
    }
}