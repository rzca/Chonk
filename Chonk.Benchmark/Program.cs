using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace Chonk.Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
        // Microsoft.SemanticKernel is not build in Release mode, so disable the validation
        var config = DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(true)));
        var summary = BenchmarkRunner.Run<Benchmarks>(config, args);

        // Use this to select benchmarks from the console:
        // var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}