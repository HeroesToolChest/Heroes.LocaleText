using Newtonsoft.Json.Linq;

namespace Heroes.LocaleText.Tests;

[TestClass]
public class GameStringTests
{
    private readonly string _testText = "Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.";
    private readonly string _testTextDeDE = "Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern <c val=\"bfd4fd\">70~~0.045~~</c> Schaden zu, verlangsamt sie um <c val=\"bfd4fd\">35%</c> und blendet sie <c val=\"bfd4fd\">1,75</c> Sek. lang.";

    [TestMethod]
    public void RawDescription_NullText_ThrowsException()
    {
        // arrange

        // act
        Action act = () => new GameStringText(null!);

        // assert
        Assert.ThrowsException<ArgumentNullException>(act);
    }

    [TestMethod]
    public void RawDescription_EmptyText_ReturnsEmptyText()
    {
        // arrange
        GameStringText gameStringText = new(string.Empty);

        // act
        string result = gameStringText.RawText;

        // assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RawDescription_TestText_ReturnsRawText()
    {
        // arrange
        GameStringText gameStringText = new(_testText);

        // act
        string result = gameStringText.RawText;

        // assert
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void PlainText_TestText_ReturnsPlainText()
    {
        // arrange
        GameStringText gameStringText = new(_testText);

        // act
        string result = gameStringText.PlainText;

        // assert
        Assert.AreEqual("Every 18 seconds, deals 125  extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithNewlines_TestText_ReturnsPlainTextWithNewlines()
    {
        // arrange
        GameStringText gameStringText = new(_testText);

        // act
        string result = gameStringText.PlainTextWithNewlines;

        // assert
        Assert.AreEqual("Every 18 seconds, deals 125<n/> extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithScaling_TestText_ReturnsPlainTextWithScaling()
    {
        // arrange
        GameStringText gameStringText = new(_testText);

        // act
        string result = gameStringText.PlainTextWithScaling;

        // assert
        Assert.AreEqual("Every 18 seconds, deals 125 (+4.5% per level)  extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithScalingWithNewlines_TestText_ReturnsPlainTextWithScalingWithNewlines()
    {
        // arrange
        GameStringText gameStringText = new(_testText);

        // act
        string result = gameStringText.PlainTextWithScalingWithNewlines;

        // assert
        Assert.AreEqual("Every 18 seconds, deals 125 (+4.5% per level)<n/> extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void ColoredText_TestText_ReturnsColoredText()
    {
        // arrange
        GameStringText gameStringText = new(_testText);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void ColoredTextWithScaling_TestText_ReturnsColoredTextWithScaling()
    {
        // arrange
        GameStringText gameStringText = new(_testText);

        // act
        string result = gameStringText.ColoredTextWithScaling;

        // assert
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125 (+4.5% per level)</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void ColoredTextWithScaling_TestTextDeDe_ReturnsColoredTextWithScaling()
    {
        // arrange
        GameStringText gameStringText = new(_testTextDeDE, StormLocale.DEDE);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern <c val=\"bfd4fd\">70</c> Schaden zu, verlangsamt sie um <c val=\"bfd4fd\">35%</c> und blendet sie <c val=\"bfd4fd\">1,75</c> Sek. lang.", result);
    }

    [TestMethod]
    public void ToString_TestText_ReturnsPlainTextWithScaling()
    {
        // arrange
        GameStringText gameStringText = new(_testText);
        string plainTextWithScaling = gameStringText.PlainTextWithScaling;

        // act
        string result = gameStringText.ToString();

        // assert
        Assert.AreEqual(plainTextWithScaling, result);
    }

    [TestMethod]
    public void GameStringLocale_TestTextDefault_ReturnsStormLocale()
    {
        // arrange
        GameStringText gameStringText = new(_testText);

        // act
        StormLocale result = gameStringText.GameStringLocale;

        // assert
        Assert.AreEqual(StormLocale.ENUS, result);
    }

    [TestMethod]
    public void GameStringLocale_TestTextDeDe_ReturnsStormLocale()
    {
        // arrange
        GameStringText gameStringText = new(_testText, StormLocale.DEDE);

        // act
        StormLocale result = gameStringText.GameStringLocale;

        // assert
        Assert.AreEqual(StormLocale.DEDE, result);
    }

    [TestMethod]
    public void FontStyleVariables_HasStyleVariables_ReturnsVars()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s>", extractFontValues: true);

        // act
        IEnumerable<string>? result = gameStringText.FontStyleValues;

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
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds.", extractFontValues: true);

        // act
        IEnumerable<string>? result = gameStringText.FontStyleConstantValues;

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
        GameStringText gameStringText = new("test text", extractFontValues: true);

        // act
        IEnumerable<string>? result = gameStringText.FontStyleValues;

        // assert
        Assert.IsTrue(gameStringText.IsFontValuesExtracted);
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
        GameStringText gameStringText = new("test text", extractFontValues: true);

        // act
        IEnumerable<string>? result = gameStringText.FontStyleConstantValues;

        // assert
        Assert.IsTrue(gameStringText.IsFontValuesExtracted);
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
        GameStringText gameStringText = new(_testText, extractFontValues: false);

        // act
        IEnumerable<string>? result = gameStringText.FontStyleValues;

        // assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FontStyleConstantVariables_ExtractSetToFalse_ReturnsEmpty()
    {
        // arrange
        GameStringText gameStringText = new(_testText, extractFontValues: false);

        // act
        IEnumerable<string>? result = gameStringText.FontStyleConstantValues;

        // assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithDictionaryConstant_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        Dictionary<string, string> keyValuePairs = [];
        keyValuePairs.Add("#TooltipNumbers", "123456");
        keyValuePairs.Add("TooltipNumbers", "123456");
        keyValuePairs.Add("TooltipNumbers2", "222222");
        keyValuePairs.Add("TooltipNumbers3", "333333");

        gameStringText.AddFontValueReplacements(FontTagType.Constant, false, keyValuePairs);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\">18</c> seconds, deals <c val=\"123456\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithDictionaryStyle_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        Dictionary<string, string> keyValuePairs = [];
        keyValuePairs.Add("StandardTooltipHeader", "123456");
        keyValuePairs.Add("StandardTooltipDetails2", "222222");

        gameStringText.AddFontValueReplacements(FontTagType.Style, false, keyValuePairs);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\">Archon </s><n/><s val=\"222222\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithDictionaryConstantPreserve_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        Dictionary<string, string> keyValuePairs = [];
        keyValuePairs.Add("#TooltipNumbers", "123456");
        keyValuePairs.Add("TooltipNumbers", "123456");
        keyValuePairs.Add("TooltipNumbers2", "222222");
        keyValuePairs.Add("TooltipNumbers3", "333333");

        gameStringText.AddFontValueReplacements(FontTagType.Constant, true, keyValuePairs);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\" hlt-name=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"123456\" hlt-name=\"TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithDictionaryStylePreserve_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        Dictionary<string, string> keyValuePairs = [];
        keyValuePairs.Add("StandardTooltipHeader", "123456");
        keyValuePairs.Add("StandardTooltipDetails2", "222222");

        gameStringText.AddFontValueReplacements(FontTagType.Style, true, keyValuePairs);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"222222\" hlt-name=\"StandardTooltipDetails2\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithTupleConstant_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("#TooltipNumbers", "123456"));
        values.Add(("TooltipNumbers2", "123456"));
        values.Add(("TooltipNumbers3", "123456"));

        gameStringText.AddFontValueReplacements(FontTagType.Constant, false, values);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\">18</c> seconds, deals <c val=\"123456\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithTupleStyle_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("StandardTooltipHeader", "123456"));
        values.Add(("StandardTooltipDetails2", "222222"));

        gameStringText.AddFontValueReplacements(FontTagType.Style, false, values);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\">Archon </s><n/><s val=\"222222\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithTupleConstantPreserve_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("#TooltipNumbers", "123456"));
        values.Add(("TooltipNumbers2", "123456"));
        values.Add(("TooltipNumbers3", "123456"));

        gameStringText.AddFontValueReplacements(FontTagType.Constant, true, values);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\" hlt-name=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"123456\" hlt-name=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithTupleStylePreserve_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("StandardTooltipHeader", "123456"));
        values.Add(("StandardTooltipDetails2", "222222"));

        gameStringText.AddFontValueReplacements(FontTagType.Style, true, values);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"222222\" hlt-name=\"StandardTooltipDetails2\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_SingleConstant_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new GameStringText("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false)
            .AddFontValueReplacement("#TooltipNumbers", "123456", FontTagType.Constant);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\">18</c> seconds, deals <c val=\"123456\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_SingleConstantPreserve_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new GameStringText("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false)
            .AddFontValueReplacement("#TooltipNumbers", "123456", FontTagType.Constant, preserveValue: true);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\" hlt-name=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"123456\" hlt-name=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_SingleStyle_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new GameStringText("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipHeader\">Cooldown: </s>", extractFontValues: false)
            .AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\">Archon </s><n/><s val=\"123456\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_SingleStylePreserve_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new GameStringText("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipHeader\">Cooldown: </s>", extractFontValues: false)
            .AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Cooldown: </s>", result);
    }

#if NET9_0_OR_GREATER
    [TestMethod]
    public void AddFontVarReplacements_WithParamsConstant_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        gameStringText.AddFontValueReplacements(FontTagType.Constant, false, ("#TooltipNumbers", "123456"), ("TooltipNumbers2", "123456"), ("TooltipNumbers3", "123456"));

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\">18</c> seconds, deals <c val=\"123456\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithParamsStyle_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        gameStringText.AddFontValueReplacements(FontTagType.Style, false, ("StandardTooltipHeader", "123456"), ("StandardTooltipDetails2", "222222"));

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\">Archon </s><n/><s val=\"222222\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithParamsConstantPreserve_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("#TooltipNumbers", "123456"));
        values.Add(("TooltipNumbers2", "123456"));
        values.Add(("TooltipNumbers3", "123456"));

        gameStringText.AddFontValueReplacements(FontTagType.Constant, preserveValues: true, values);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("Every <c val=\"123456\" hlt-name=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"123456\" hlt-name=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void AddFontVarReplacements_WithParamsStylePreserve_ReturnsResultWithReplace()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);

        List<(string, string)> values = [];
        values.Add(("StandardTooltipHeader", "123456"));
        values.Add(("StandardTooltipDetails2", "222222"));

        gameStringText.AddFontValueReplacements(FontTagType.Style, preserveValues: true, values);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"222222\" hlt-name=\"StandardTooltipDetails2\">Cooldown: </s>", result);
    }
#endif

    [TestMethod]
    public void AddFontVarReplacement_ExtractFontValuesThenAddReplacements_ColorDescriptionsShouldBeUpdated()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: true);
        if (gameStringText.IsFontValuesExtracted)
        {
            // extract
            _ = gameStringText.FontStyleValues.ToList();
        }

        // then add
        gameStringText.AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);

        // act
        string resultRaw = gameStringText.RawText;
        string resultPlainText = gameStringText.PlainText;
        string resultPlainTextWithScaling = gameStringText.PlainTextWithScaling;
        string resultPlainTextWithNewlines = gameStringText.PlainTextWithNewlines;
        string resultPlainTextWithScalingWithNewlines = gameStringText.PlainTextWithScalingWithNewlines;
        string resultColorText = gameStringText.ColoredText;
        string resultColoredTextWithScaling = gameStringText.ColoredTextWithScaling;

        // assert
        Assert.IsTrue(gameStringText.IsFontValuesExtracted);
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
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);
        string originalColorText = gameStringText.ColoredText;

        // then add
        gameStringText.AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);

        // act
        string result = gameStringText.ColoredText;

        // assert
        Assert.AreNotEqual(originalColorText, result);
        Assert.AreEqual("<s val=\"123456\" hlt-name=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", result);
    }

    [TestMethod]
    public void AddFontVarReplacement_PlainTextThenAddReplacements_NoChanges()
    {
        // arrange
        GameStringText gameStringText = new("<s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: false);
        string originalPlainText = gameStringText.PlainText;
        string originalPlaintTextWithScaling = gameStringText.PlainTextWithScaling;
        string originalPlainTextWithNewlines = gameStringText.PlainTextWithNewlines;
        string originalPlainTextWithScalingWithNewlines = gameStringText.PlainTextWithScalingWithNewlines;

        // then add
        gameStringText.AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);

        // act
        string result = gameStringText.PlainText;
        string resultPlainTextWithScaling = gameStringText.PlainTextWithScaling;
        string resultPlainTextWithNewlines = gameStringText.PlainTextWithNewlines;
        string resultPlainTextWithScalingWithNewlines = gameStringText.PlainTextWithScalingWithNewlines;

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
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, <s val=\"StandardTooltipHeader\">Archon </s><n/><s val=\"StandardTooltipDetails2\">Cooldown: </s>", extractFontValues: true);

        List<string> originalFontStyleValues = [.. gameStringText.FontStyleValues!];
        List<string> originalFontStyleConstantValues = [.. gameStringText.FontStyleConstantValues!];

        // then add
        gameStringText.AddFontValueReplacement("StandardTooltipHeader", "123456", FontTagType.Style, preserveValue: true);
        gameStringText.AddFontValueReplacement("#TooltipNumbers", "aaaaaaaa", FontTagType.Constant, preserveValue: true);
        _ = gameStringText.RawText;

        // act
        List<string> fontStyleValues = [.. gameStringText.FontStyleValues!];
        List<string> fontStyleConstantValues = [.. gameStringText.FontStyleConstantValues!];

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
        GameStringText gameStringText = new("Attaquer un héros ralenti, immobilisé ou étourdi augmente les dégâts des attaques de base de Grisetête de <c val=\"#TooltipNumbers\">0 %</c> pendant <c val=\"#TooltipNumbers\">0</c> secondes. Ce bonus passe à <c val=\"#TooltipNumbers\">0 %</c> en forme de <c val=\"#ColorViolet »>worgen</c>.", gameStringLocale: StormLocale.DEDE, extractFontValues: true);

        // act
        List<string> originalFontStyleConstantValues = [.. gameStringText.FontStyleConstantValues!];

        // assert
        Assert.HasCount(1, originalFontStyleConstantValues); // only one, the other is broken by the french quote
        Assert.Contains("#TooltipNumbers", originalFontStyleConstantValues);
    }
}