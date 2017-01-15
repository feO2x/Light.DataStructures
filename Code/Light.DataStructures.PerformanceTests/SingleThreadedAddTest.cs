using System.Collections.Generic;
using System.Diagnostics;

namespace Light.DataStructures.PerformanceTests
{
    public sealed class SingleThreadedAddTest : IPerformanceTest
    {
        private const int NumberOfKeys = 10000000;

        public PerformanceTestResults Run(IDictionary<int, object> dictionary)
        {
            var keys = IntKeyCreator.CreateKeys(NumberOfKeys);
            var stopwatch = Stopwatch.StartNew();
            foreach (var key in keys)
            {
                dictionary.Add(key, new object());
            }
            stopwatch.Stop();

            return new PerformanceTestResults(nameof(SingleThreadedAddTest), dictionary.GetType().Name, stopwatch.Elapsed);
        }
    }
}