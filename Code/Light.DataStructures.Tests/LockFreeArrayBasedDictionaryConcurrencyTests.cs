using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Light.DataStructures.DataRaceLogging;
using Xunit;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.ConcurrencyTests)]
    public sealed class LockFreeArrayBasedDictionaryConcurrencyTests
    {
        [Fact]
        public void ConcurrentAddTest()
        {
            var logger = new ConcurrentLogger();
            LoggingHelper.Logger = logger;
            var dictionary = new LockFreeArrayBasedDictionary<int, object>();
            var processorCount = Environment.ProcessorCount;
            var entryCount = processorCount * 100000;
            var allNumbers = Enumerable.Range(1, entryCount).ToArray();
            var groupsPerTask = allNumbers.GroupBy(number => number % processorCount)
                                          .ToArray();
            Parallel.ForEach(groupsPerTask, group =>
                                            {
                                                foreach (var number in group)
                                                {
                                                    var addResult = dictionary.TryAdd(number, new object());
                                                    addResult.Should().BeTrue();
                                                }
                                            });

            LoggingHelper.Logger = null;
            logger.WriteLogMessages(new StreamWriter("ConcurrentAddLog.txt"));
            dictionary.Count.Should().Be(allNumbers.Length);
            dictionary.Should().ContainKeys(allNumbers);
        }
    }
}