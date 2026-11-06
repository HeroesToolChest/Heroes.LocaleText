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
    // | NoReplacementRawDescription | 317.7 ns | 2.77 ns | 2.59 ns | 0.0215 |     360 B |
    // | NoReplacementColorText      | 323.6 ns | 3.20 ns | 2.99 ns | 0.0405 |     680 B |
    // | NoReplacementColorText2     | 480.9 ns | 2.99 ns | 2.50 ns | 0.0401 |     680 B |
    // | WithReplacement             | 536.7 ns | 3.89 ns | 3.64 ns | 0.0553 |     936 B |
    // | WithReplacementPreserve     | 602.1 ns | 5.32 ns | 4.44 ns | 0.0620 |    1048 B |
    // | NestTags                    | 269.0 ns | 2.08 ns | 1.95 ns | 0.0396 |     664 B |
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

#if NET9_0_OR_GREATER
    [Benchmark]
    public string WithReplacementParams()
    {
        GameStringText gameStringText = new("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", extractFontValues: false);

        gameStringText.AddFontValueReplacements(FontTagType.Constant, false, _keyValuePairs2);

        return gameStringText.ColoredText;
    }
#endif

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
