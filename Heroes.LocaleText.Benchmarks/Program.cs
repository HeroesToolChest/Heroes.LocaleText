// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using Heroes.LocaleText.Benchmarks;

BenchmarkRunner.Run<LocalTextBenchmarks>();