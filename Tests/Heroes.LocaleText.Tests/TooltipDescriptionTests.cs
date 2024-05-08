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

        // assert
        Action act = () => new TooltipDescription(null!);

        // act
        Assert.ThrowsException<ArgumentNullException>(act);
    }

    [TestMethod]
    public void RawDescription_EmptyText_ReturnsEmptyDescription()
    {
        // arrange
        TooltipDescription tooltipDescription = new(string.Empty);

        // assert
        string result = tooltipDescription.RawDescription;

        // act
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RawDescription_TestText_ReturnsRawDescription()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // assert
        string result = tooltipDescription.RawDescription;

        // act
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void PlainText_TestText_ReturnsPlainText()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // assert
        string result = tooltipDescription.PlainText;

        // act
        Assert.AreEqual("Every 18 seconds, deals 125  extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithNewlines_TestText_ReturnsPlainTextWithNewlines()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // assert
        string result = tooltipDescription.PlainTextWithNewlines;

        // act
        Assert.AreEqual("Every 18 seconds, deals 125<n/> extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithScaling_TestText_ReturnsPlainTextWithScaling()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // assert
        string result = tooltipDescription.PlainTextWithScaling;

        // act
        Assert.AreEqual("Every 18 seconds, deals 125 (+4.5% per level)  extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void PlainTextWithScalingWithNewlines_TestText_ReturnsPlainTextWithScalingWithNewlines()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // assert
        string result = tooltipDescription.PlainTextWithScalingWithNewlines;

        // act
        Assert.AreEqual("Every 18 seconds, deals 125 (+4.5% per level)<n/> extra damage every 2.75 seconds.", result);
    }

    [TestMethod]
    public void ColoredText_TestText_ReturnsColoredText()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // assert
        string result = tooltipDescription.ColoredText;

        // act
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void ColoredTextWithScaling_TestText_ReturnsColoredTextWithScaling()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // assert
        string result = tooltipDescription.ColoredTextWithScaling;

        // act
        Assert.AreEqual("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125 (+4.5% per level)</c><n/> extra damage every <c val=\"#TooltipNumbers\">2.75</c> seconds.", result);
    }

    [TestMethod]
    public void ColoredTextWithScaling_TestTextDeDe_ReturnsColoredTextWithScaling()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testTextDeDE, StormLocale.DEDE);

        // assert
        string result = tooltipDescription.ColoredText;

        // act
        Assert.AreEqual("Wirft einen Schneeball, der alle Gegner in einem Bereich trifft. Fügt getroffenen Gegnern <c val=\"bfd4fd\">70</c> Schaden zu, verlangsamt sie um <c val=\"bfd4fd\">35%</c> und blendet sie <c val=\"bfd4fd\">1,75</c> Sek. lang.", result);
    }

    [TestMethod]
    public void ToString_TestText_ReturnsPlainTextWithScaling()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);
        string plainTextWithScaling = tooltipDescription.PlainTextWithScaling;

        // assert
        string result = tooltipDescription.ToString();

        // act
        Assert.AreEqual(plainTextWithScaling, result);
    }

    [TestMethod]
    public void GameStringLocale_TestTextDefault_ReturnsStormLocale()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText);

        // assert
        StormLocale result = tooltipDescription.GameStringLocale;

        // act
        Assert.AreEqual(StormLocale.ENUS, result);
    }

    [TestMethod]
    public void GameStringLocale_TestTextDeDe_ReturnsStormLocale()
    {
        // arrange
        TooltipDescription tooltipDescription = new(_testText, StormLocale.DEDE);

        // assert
        StormLocale result = tooltipDescription.GameStringLocale;

        // act
        Assert.AreEqual(StormLocale.DEDE, result);
    }
}