using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Light.DataStructures.LockFreeArrayBasedServices;
using Xunit;

#if CONCURRENT_LOGGING
using System.IO;
using Light.DataStructures.DataRaceLogging;
#endif

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.ConcurrencyTests)]
    public sealed class LockFreeArrayBasedDictionaryConcurrencyTests
    {
        [Fact]
        public void ConcurrentAddTest()
        {
            // Arrange
            var options = new LockFreeArrayBasedDictionary<int, object>.Options { BackgroundCopyTaskFactory = new FactoryCreatingAttachedChildTasks() };
#if CONCURRENT_LOGGING
            var logger = new ConcurrentLogger();
            options.GrowArrayProcessFactory = new LoggingDecoratorFactoryForGrowArrayProcesses<int, object>(new GrowArrayProcessFactory<int, object>(), logger);
#endif

            var dictionary = new LockFreeArrayBasedDictionary<int, object>(options);
#if CONCURRENT_LOGGING
            dictionary.Logger = logger;
#endif
            var processorCount = Environment.ProcessorCount;
            var entryCount = processorCount * 100000;
            var allNumbers = Enumerable.Range(1, entryCount).ToArray();
            var groupsPerTask = allNumbers.GroupBy(number => number % processorCount)
                                          .ToArray();

            // Act
            try
            {
                Parallel.ForEach(groupsPerTask, group =>
                                                {
                                                    foreach (var number in group)
                                                    {
                                                        if (dictionary.TryAdd(number, new object()))
                                                            continue;

                                                        var errorMessage = $"Could not add entry {number}.";
#if CONCURRENT_LOGGING
                                                        logger.Log(errorMessage);
#endif
                                                        throw new AssertionFailedException(errorMessage);
                                                    }
                                                });


                // Assert
                dictionary.Count.Should().Be(allNumbers.Length);
                dictionary.Should().ContainKeys(allNumbers);
            }
            catch (Exception)
            {
#if CONCURRENT_LOGGING
                logger.WriteLogMessages(new StreamWriter("ConcurrentAddLog.txt"));
#endif
                throw;
            }
        }
    }
}