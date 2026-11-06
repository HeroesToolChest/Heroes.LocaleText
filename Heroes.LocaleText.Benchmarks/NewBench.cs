using BenchmarkDotNet.Attributes;

namespace Heroes.LocaleText.Benchmarks;

[MemoryDiagnoser]
public class NewBench
{
    // | Method     | Mean      | Error    | StdDev   | Gen0   | Allocated |
    // |----------- |----------:|---------:|---------:|-------:|----------:|
    // | GSPShort   |  17.49 ns | 0.383 ns | 0.680 ns | 0.0052 |      88 B |
    // | GSP        | 366.36 ns | 3.159 ns | 2.638 ns | 0.0196 |     328 B |
    // | GSPColored | 547.95 ns | 9.093 ns | 8.506 ns | 0.0410 |     688 B |
    [Benchmark]
    public string GSPShort()
    {
        GameStringParser gameStringText = GameStringParser.GetInstance("Every 10", StormLocale.ENUS, extractFontVars: false);

        return gameStringText.GetRawText();
    }

    [Benchmark]
    public string GSP()
    {
        GameStringParser gameStringText = GameStringParser.GetInstance("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", StormLocale.ENUS, extractFontVars: false);

        return gameStringText.GetRawText();
    }

    [Benchmark]
    public string GSPColored()
    {
        GameStringParser gameStringText = GameStringParser.GetInstance("Every <c val=\"#TooltipNumbers\">18</c> seconds, deals <c val=\"#TooltipNumbers\">125~~0.045~~</c><n/> extra damage every <c val=\"#TooltipOther\">2.75</c> seconds.", StormLocale.ENUS, extractFontVars: false);

        return gameStringText.GetColoredText(includeScaling: true);
    }
}
