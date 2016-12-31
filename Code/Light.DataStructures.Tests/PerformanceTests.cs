using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Light.DataStructures.LockFreeArrayBasedServices;
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

        private static readonly Dictionary<int, List<int>> PerThreadKeyGroups = new Dictionary<int, List<int>>();

        static PerformanceTests()
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
                new object[] { new LockFreeArrayBasedDictionary<int, object>() },
                new object[] { new Dictionary<int, object>() },
                new object[] { new ConcurrentDictionary<int, object>() }
            };

        [Theory]
        [MemberData(nameof(MultiThreadedDictionaryInstances))]
        public void MultiThreadedAdd(IDictionary<int, object> dictionary)
        {
            Parallel.ForEach(PerThreadKeyGroups, kvp =>
                                                 {
                                                     foreach (var number in kvp.Value)
                                                     {
                                                         dictionary.Add(number, new object());
                                                     }
                                                 });
        }

        public static readonly TestData MultiThreadedDictionaryInstances =
            new[]
            {
                new object[] { new LockFreeArrayBasedDictionary<int, object>(new LockFreeArrayBasedDictionary<int, object>.Options { BackgroundCopyTaskFactory = new FactoryCreatingAttachedChildTasks() }) },
                new object[] { new ConcurrentDictionary<int, object>() }
            };
    }
}