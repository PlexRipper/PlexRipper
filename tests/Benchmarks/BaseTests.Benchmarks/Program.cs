using BenchmarkDotNet.Running;

namespace BaseTests.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<FakeApiDataGenerateBenchmark>();
    }
}
