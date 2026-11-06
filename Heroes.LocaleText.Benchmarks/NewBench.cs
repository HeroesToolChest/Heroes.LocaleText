using BenchmarkDotNet.Attributes;

namespace Heroes.LocaleText.Benchmarks;

[MemoryDiagnoser]
public class NewBench
{
    // | Method     | Mean       | Error     | StdDev    | Gen0   | Allocated |
    // |----------- |-----------:|----------:|----------:|-------:|----------:|
    // | GSPShort   |   7.339 ns | 0.0875 ns | 0.0818 ns | 0.0062 |     104 B |
    // | GSP        | 301.800 ns | 1.1719 ns | 1.0389 ns | 0.0205 |     344 B |
    // | GSPColored | 456.325 ns | 2.7066 ns | 2.5317 ns | 0.0420 |     704 B |
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
