using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Light.DataStructures.DataRaceLogging;
using Light.DataStructures.LockFreeArrayBasedServices;
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
            Logging.Logger = logger;
            var options = new LockFreeArrayBasedDictionary<int, object>.Options { BackgroundCopyTaskFactory = new FactoryCreatingAttachedChildTasks() };
            var dictionary = new LockFreeArrayBasedDictionary<int, object>(options);
            var processorCount = Environment.ProcessorCount;
            var entryCount = processorCount * 100000;
            var allNumbers = Enumerable.Range(1, entryCount).ToArray();
            var groupsPerTask = allNumbers.GroupBy(number => number % processorCount)
                                          .ToArray();
            try
            {
                Parallel.ForEach(groupsPerTask, group =>
                                                {
                                                    foreach (var number in group)
                                                    {
                                                        if (dictionary.TryAdd(number, new object()))
                                                            continue;

                                                        var errorMessage = $"Could not add entry {number}.";
                                                        logger.Log(errorMessage);
                                                        throw new AssertionFailedException(errorMessage);
                                                    }
                                                });


                dictionary.Count.Should().Be(allNumbers.Length);
                dictionary.Should().ContainKeys(allNumbers);
            }
            catch (Exception)
            {
                Logging.Logger = null;
                logger.WriteLogMessages(new StreamWriter("ConcurrentAddLog.txt"));
                throw;
            }
        }
    }
}