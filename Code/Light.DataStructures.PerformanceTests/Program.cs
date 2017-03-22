using BenchmarkDotNet.Running;
using Light.DataStructures.PerformanceTests.LockFreeArrayBasedDictionaryTests;

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