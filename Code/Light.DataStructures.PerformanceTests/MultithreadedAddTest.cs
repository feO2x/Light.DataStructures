using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Light.DataStructures.PerformanceTests
{
    public sealed class MultiThreadedAddTest : IPerformanceTest
    {
        private const int NumberOfKeys = 10000000;

        private static readonly int[] Keys = Enumerable.Range(1, NumberOfKeys)
                                                       .GroupBy(number => number % 27)
                                                       .SelectMany(group => group)
                                                       .ToArray();

        private static readonly Dictionary<int, List<int>> PerThreadKeyGroups = new Dictionary<int, List<int>>();

        static MultiThreadedAddTest()
        {
            var numberOfProcessors = Environment.ProcessorCount;
            for (var i = 0; i < numberOfProcessors; i++)
            {
                PerThreadKeyGroups.Add(i, new List<int>());
            }

            for (var i = 0; i < NumberOfKeys; i++)
            {
                var targetKey = i % numberOfProcessors;
                PerThreadKeyGroups[targetKey].Add(Keys[i]);
            }
        }

        public PerformanceTestResults Run(IDictionary<int, object> dictionary)
        {
            var stopwatch = Stopwatch.StartNew();
            Parallel.ForEach(PerThreadKeyGroups, kvp =>
                                                 {
                                                     foreach (var number in kvp.Value)
                                                     {
                                                         dictionary.Add(number, new object());
                                                     }
                                                 });
            stopwatch.Stop();

            return new PerformanceTestResults(nameof(MultiThreadedAddTest), dictionary.GetType().Name, stopwatch.Elapsed);
        }
    }
}