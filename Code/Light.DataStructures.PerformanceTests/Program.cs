using BenchmarkDotNet.Running;
using Light.DataStructures.PerformanceTests.PrecompiledDictionaryTests;

namespace Light.DataStructures.PerformanceTests
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<T4TemplateGeneratedDictionaryReadTest>();
        }
    }
}