using BenchmarkDotNet.Attributes;

namespace Heroes.LocaleText.Benchmarks;

[MemoryDiagnoser]
public class LocalTextBenchmarks
{
    private readonly Dictionary<string, string> _keyValuePairs = [];

    public LocalTextBenchmarks()
    {
        _keyValuePairs.Add("#TooltipNumbers", "123456");
        _keyValuePairs.Add("TooltipNumbers2", "222222");
        _keyValuePairs.Add("TooltipNumbers3", "333333");
    }

    [Benchmark]
    public string NoReplacementRawDescription()
    {
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        return tooltipDescription.RawDescription;
    }

    [Benchmark]
    public string NoReplacementColorText()
    {
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        return tooltipDescription.ColoredText;
    }

    [Benchmark]
    public string WithReplacement()
    {
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        tooltipDescription.AddFontValueReplacements(_keyValuePairs, FontTagType.Constant);

        return tooltipDescription.ColoredText;
    }
}
