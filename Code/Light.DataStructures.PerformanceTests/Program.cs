using BenchmarkDotNet.Running;

namespace Light.DataStructures.PerformanceTests
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<MultiThreadedReadAndAddTest>();
        }
    }
}