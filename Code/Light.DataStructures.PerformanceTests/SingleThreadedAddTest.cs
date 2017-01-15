using System.Collections.Generic;
using System.Diagnostics;
using Light.GuardClauses;

namespace Light.DataStructures.PerformanceTests
{
    public sealed class SingleThreadedAddTest : IPerformanceTest
    {
        private readonly int _numberOfKeys;

        public SingleThreadedAddTest(int numberOfKeys)
        {
            numberOfKeys.MustNotBeLessThan(0);

            _numberOfKeys = numberOfKeys;
        }

        public PerformanceTestResults Run(IDictionary<int, object> dictionary)
        {
            var keys = IntKeyTestInfo.CreateKeys(_numberOfKeys);
            var stopwatch = Stopwatch.StartNew();
            foreach (var key in keys)
            {
                dictionary.Add(key, new object());
            }
            stopwatch.Stop();

            return new PerformanceTestResults(nameof(SingleThreadedAddTest), dictionary.GetType().Name, stopwatch.Elapsed);
        }

        public static IPerformanceTest Create(IDictionary<string, string> parsedCommandArguments)
        {
            var numberOfKeys = IntKeyTestInfo.GetNumberOfKeysFromArguments(parsedCommandArguments);
            return new SingleThreadedAddTest(numberOfKeys);
        }
    }
}