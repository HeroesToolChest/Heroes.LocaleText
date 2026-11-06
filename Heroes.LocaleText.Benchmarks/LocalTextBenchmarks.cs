using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Heroes.LocaleText.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
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

    // | Method                      | Mean     | Error   | StdDev  | Gen0   | Allocated |
    // |---------------------------- |---------:|--------:|--------:|-------:|----------:|
    // | NoReplacementRawDescription | 300.4 ns | 1.03 ns | 0.96 ns | 0.0205 |     344 B |
    // | NoReplacementColorText      | 312.0 ns | 0.89 ns | 0.84 ns | 0.0396 |     664 B |
    // | NoReplacementColorText2     | 463.0 ns | 4.92 ns | 4.60 ns | 0.0396 |     664 B |
    // | WithReplacement             | 534.3 ns | 2.36 ns | 2.21 ns | 0.0544 |     920 B |
    // | WithReplacementParams       | 500.0 ns | 1.66 ns | 1.39 ns | 0.0515 |     872 B |
    // | WithReplacementPreserve     | 578.0 ns | 4.98 ns | 4.41 ns | 0.0610 |    1032 B |
    // | NestTags                    | 263.6 ns | 0.90 ns | 0.75 ns | 0.0386 |     648 B |
    [Benchmark]
    public string NoReplacementRawDescription()
    {
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        return gameStringText.RawText;
    }

    [Benchmark]
    public string NoReplacementColorText()
    {
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        return gameStringText.ColoredText;
    }

    [Benchmark]
    public string NoReplacementColorText2()
    {
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        string x = gameStringText.RawText;

        return gameStringText.ColoredText;
    }

    [Benchmark]
    public string WithReplacement()
    {
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        gameStringText.AddFontValueReplacements(FontTagType.Constant, false, _keyValuePairs2);

        return gameStringText.ColoredText;
    }

    [Benchmark]
    public string WithReplacementParams()
    {
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        gameStringText.AddFontValueReplacements(FontTagType.Constant, false, ("#TooltipNumbers", "123456"), ("TooltipNumbers2", "222222"), ("TooltipNumbers3", "333333"));

        return gameStringText.ColoredText;
    }

    [Benchmark]
    public string WithReplacementPreserve()
    {
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        gameStringText.AddFontValueReplacements(FontTagType.Constant, true, _keyValuePairs);

        return gameStringText.ColoredText;
    }

    [Benchmark]
    public string NestTags()
    {
        GameStringText gameStringText = new("<c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">30%</c> points</c>");

        string x = gameStringText.RawText;

        return gameStringText.PlainText;
    }
}
