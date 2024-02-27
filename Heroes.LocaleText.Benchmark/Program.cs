using BenchmarkDotNet.Running;
using Heroes.LocaleText;
using Heroes.LocaleText.Benchmark;

//DescriptionParserCopy dp = DescriptionParserCopy.Validate("<c val=\"#TooltipNumbers\">120~~0.04~~</c><n/> damage per second for <c val=\"#TooltipNumbers\">120~~0.045~~</c> damage");

//Console.WriteLine(dp.GetRawDescription());

//Console.ReadKey();
BenchmarkRunner.Run<TooltipsBenchmarks>();