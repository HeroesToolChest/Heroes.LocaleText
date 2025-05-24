using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Heroes.LocaleText.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class LocalTextBenchmarks
{
    private readonly Dictionary<string, string> _keyValuePairs = [];
    private readonly List<(string, string)> _keyValuePairs2 = [];

    public LocalTextBenchmarks()
    {
        _keyValuePairs.Add("#TooltipNumbers", "123456");
        _keyValuePairs.Add("TooltipNumbers2", "222222");
        _keyValuePairs.Add("TooltipNumbers3", "333333");

        _keyValuePairs2.Add(("#TooltipNumbers", "123456"));
        _keyValuePairs2.Add(("TooltipNumbers2", "222222"));
        _keyValuePairs2.Add(("TooltipNumbers3", "333333"));
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
    public string NoReplacementColorText2()
    {
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        string x = tooltipDescription.RawDescription;

        return tooltipDescription.ColoredText;
    }

    [Benchmark]
    public string WithReplacement()
    {
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        tooltipDescription.AddFontValueReplacements(_keyValuePairs2, FontTagType.Constant);

        return tooltipDescription.ColoredText;
    }

#if NET9_0_OR_GREATER
    [Benchmark]
    public string WithReplacementParams()
    {
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        tooltipDescription.AddFontValueReplacements(FontTagType.Constant, false, _keyValuePairs2);

        return tooltipDescription.ColoredText;
    }
#endif

    [Benchmark]
    public string WithReplacementPreserve()
    {
        TooltipDescription tooltipDescription = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        tooltipDescription.AddFontValueReplacements(_keyValuePairs, FontTagType.Constant, preserveValues: true);

        return tooltipDescription.ColoredText;
    }
}
