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
    private readonly string _nestedTagDescription2 = "<c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">30%</c> points <c val=\"#TooltipNumbers\">30%</c> charges</c>";
    private readonly string _plainTextScaling1 = "<c val=\"#TooltipNumbers\">120~~0.04~~</c><n/> damage per second";
    private readonly string _coloredText1 = "<c val=\"#TooltipNumbers\">100~~0.04~~</c><n/> damage per second<n/>";
    private readonly string _dvaMechSelfDestruct = "Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\">4</c> seconds. Deals <c val=\"#TooltipNumbers\">400</c> to <c val=\"#TooltipNumbers\">1200</c> damage in a large area, depending on distance from center. Only deals <c val=\"#TooltipNumbers\">50%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">30%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>";

    public TooltipsBenchmarks()
    {
    }

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
