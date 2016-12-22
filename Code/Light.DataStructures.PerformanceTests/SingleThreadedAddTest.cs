using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Light.DataStructures.PerformanceTests
{
    public sealed class SingleThreadedAddTest
    {
        private const int NumberOfKeys = 10000000;

        private static readonly int[] Keys = Enumerable.Range(1, NumberOfKeys)
                                                       .GroupBy(number => number % 27)
                                                       .SelectMany(group => group)
                                                       .ToArray();

        public PerformanceTestResults Run(IDictionary<int, object> dictionary)
        {
            var stopwatch = Stopwatch.StartNew();
            foreach (var key in Keys)
            {
                dictionary.Add(key, new object());
            }
            stopwatch.Stop();

            return new PerformanceTestResults(nameof(SingleThreadedAddTest), dictionary.GetType().Name, stopwatch.Elapsed);
        }
    }
}