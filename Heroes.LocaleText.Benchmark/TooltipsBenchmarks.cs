using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heroes.LocaleText.Benchmark;

[MemoryDiagnoser]
public class TooltipsBenchmarks
{
    private readonly string _extraSpacesTagDescription1 = "<c  val=\"#TooltipQuest\"> Repeatable Quest:</c> Gain<c val=\"#TooltipNumbers\">10</c>";
    private readonly string _nestedTagDescription1 = "<c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">30%</c> points</c>";
    private readonly string _nestedTagDescription2 = "<c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">30%</c> points <c val=\"#TooltipNumbers\">30%</c> charges</c>";
    private readonly string _plainTextScaling1 = "<c val=\"#TooltipNumbers\">120~~0.04~~</c><n/> damage per second";
    private readonly string _plainTextScalingNewline1 = "<c val=\"#TooltipNumbers\">120~~0.04~~</c><n/> damage per second";
    private readonly string _plainTextScalingDoubleScaleNewline1 = "<c val=\"#TooltipNumbers\">120~~0.04~~</c><n/> damage per second for <c val=\"#TooltipNumbers\">120~~0.045~~</c> damage";
    private readonly string _coloredText1 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c><n/> damage per second<n/>";
    private readonly string _dvaMechSelfDestruct = "Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\">4</c> seconds. Deals <c val=\"#TooltipNumbers\">400</c> to <c val=\"#TooltipNumbers\">1200</c> damage in a large area, depending on distance from center. Only deals <c val=\"#TooltipNumbers\">50%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">30%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>";

    public TooltipsBenchmarks()
    {
    }

    //[Benchmark]
    //public (string, string) Old()
    //{
    //    string raw = DescriptionParserOld.Validate(_plainTextScalingDoubleScaleNewline1);
    //    string plain = DescriptionParserOld.GetPlainText(raw, true, true);

    //    return (raw, plain);
    //}

    //[Benchmark]
    //public string OldSingle()
    //{
    //    return DescriptionParserOld.Validate(_plainTextScalingDoubleScaleNewline1);
    //}

    [Benchmark]
    public (string, string) New()
    {
        DescriptionParser dp = DescriptionParser.Validate(_plainTextScalingDoubleScaleNewline1);

        string raw = dp.GetRawDescription();
        string plain = dp.GetPlainText(true, true);

        return (raw, plain);
    }

    [Benchmark]
    public string NewSingle()
    {
        return DescriptionParser.Validate(_plainTextScalingDoubleScaleNewline1).GetRawDescription();

    }

    //[Benchmark]
    //public Range List()
    //{
    //    List<TextRange> list = new List<TextRange>();
    //    list.Add(new TextRange(new Range(0, 4), TextType.ScalingTag));
    //    list.Add(new TextRange(new Range(0, 4), TextType.ScalingTag));
    //    list.Add(new TextRange(new Range(0, 4), TextType.ScalingTag));
    //    list.Add(new TextRange(new Range(0, 4), TextType.ScalingTag));
    //    list.Add(new TextRange(new Range(0, 4), TextType.ScalingTag));

    //    return list[4].Range;
    //}


    //[Benchmark]
    //public Range Stack()
    //{
    //    Stack<TextRange> list = new Stack<TextRange>();
    //    list.Push(new TextRange(new Range(0, 4), TextType.ScalingTag));
    //    list.Push(new TextRange(new Range(0, 4), TextType.ScalingTag));
    //    list.Push(new TextRange(new Range(0, 4), TextType.ScalingTag));
    //    list.Push(new TextRange(new Range(0, 4), TextType.ScalingTag));
    //    list.Push(new TextRange(new Range(0, 4), TextType.ScalingTag));

    //    list.Pop();
    //    list.Pop();
    //    list.Pop();
    //    list.Pop();
    //    return list.Peek().Range;
    //}
    //[Benchmark]
    //public string Test()
    //{
    //    TooltipDescription td = new(_dvaMechSelfDestruct);

    //    return td.ColoredText;
    //    //string a = DescriptionParser.Validate(_dvaMechSelfDestruct);
    //    //return DescriptionParser.GetColoredText(a, true);
    //}

    //[Benchmark]
    //public string Test2()
    //{
    //    TooltipDescription td = new(_coloredText1);

    //    return td.ColoredText;

    //    //string a = DescriptionParser.Validate(_coloredText1);
    //    //return DescriptionParser.GetColoredText(a, true);
    //}


    //[Benchmark]
    //public string Old()
    //{
    //    string a = DescriptionValidator.Validate(_dvaMechSelfDestruct);
    //    return DescriptionValidator.GetColoredText(a, false);
    //}

    //[Benchmark]
    //public string New()
    //{
    //    string a = DescriptionParser.Validate(_dvaMechSelfDestruct);
    //    return DescriptionParser.GetColoredText(a, false);
    //}
}
