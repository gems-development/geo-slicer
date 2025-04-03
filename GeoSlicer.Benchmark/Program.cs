using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using GeoSlicer.Benchmark.Benchmarks;
using GeoSlicer.DivideAndRuleSlicers.OppositesIndexesGivers;

namespace GeoSlicer.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ManualConfig config = new ManualConfig()
                .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                .AddValidator(JitOptimizationsValidator.DontFailOnError)
                .AddLogger(ConsoleLogger.Default)
                .AddColumnProvider(DefaultColumnProviders.Instance)
                .AddColumn(StatisticColumn.Max);
            BenchmarkRunner.Run<OppositeSlicerKazanBench>(config);
        }
    }
}