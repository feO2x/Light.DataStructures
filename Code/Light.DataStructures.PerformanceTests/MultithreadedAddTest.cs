using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Light.DataStructures.PerformanceTests
{
    public sealed class MultiThreadedAddTest : IPerformanceTest
    {
        private readonly int _numberOfKeys;

        public MultiThreadedAddTest(int numberOfKeys)
        {
            _numberOfKeys = numberOfKeys;
        }

        public PerformanceTestResults Run(IDictionary<int, object> dictionary)
        {
            // Set up data for threads
            var keys = IntKeyTestInfo.CreateKeys(_numberOfKeys);

            var numberOfProcessors = Environment.ProcessorCount;
            var perThreadKeyGroups = new Dictionary<int, List<int>>();
            for (var i = 0; i < numberOfProcessors; i++)
            {
                perThreadKeyGroups.Add(i, new List<int>());
            }

            for (var i = 0; i < _numberOfKeys; i++)
            {
                var targetKey = i % numberOfProcessors;
                perThreadKeyGroups[targetKey].Add(keys[i]);
            }

            // Run the actual test
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

        public static IPerformanceTest Create(IDictionary<string, string> parsedCommandArguments)
        {
            var numberOfKeys = IntKeyTestInfo.GetNumberOfKeysFromArguments(parsedCommandArguments);
            return new MultiThreadedAddTest(numberOfKeys);
        }
    }
}