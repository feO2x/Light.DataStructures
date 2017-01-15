using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Light.DataStructures.PerformanceTests
{
    public sealed class MultiThreadedAddTest : IPerformanceTest
    {
        private const int NumberOfKeys = 10000000;

        public PerformanceTestResults Run(IDictionary<int, object> dictionary)
        {
            var keys = IntKeyCreator.CreateKeys(NumberOfKeys);

            var numberOfProcessors = Environment.ProcessorCount;
            var perThreadKeyGroups = new Dictionary<int, List<int>>();
            for (var i = 0; i < numberOfProcessors; i++)
            {
                perThreadKeyGroups.Add(i, new List<int>());
            }

            for (var i = 0; i < NumberOfKeys; i++)
            {
                var targetKey = i % numberOfProcessors;
                perThreadKeyGroups[targetKey].Add(keys[i]);
            }

            var stopwatch = Stopwatch.StartNew();
            Parallel.ForEach(perThreadKeyGroups, kvp =>
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