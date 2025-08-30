using BenchmarkDotNet.Running;

namespace Reaparr.BaseTests.Benchmarks;

public class ProgramBenchmarks
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<FakeApiDataGenerateBenchmark>();
    }
}
