using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.PerformanceTests)]
    public sealed class PerformanceTests
    {
        private const int NumberOfKeys = 10000000;

        private static readonly int[] Keys = Enumerable.Range(1, NumberOfKeys)
                                                       .GroupBy(number => number % 27)
                                                       .SelectMany(group => group)
                                                       .ToArray();

        [Theory]
        [MemberData(nameof(SingleThreadedDictionaryInstances))]
        public void SingleThreadedAdd(IDictionary<int, object> dictionary)
        {
            foreach (var key in Keys)
            {
                dictionary.Add(key, new object());
            }
        }

        public static readonly TestData SingleThreadedDictionaryInstances =
            new[]
            {
                new object[] { new Dictionary<int, object>() },
                new object[] { new ConcurrentDictionary<int, object>() },
                new object[] { new LockFreeArrayBasedDictionary<int, object>() }
            };
    }
}