using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Light.DataStructures.LockFreeArrayBasedServices;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.FunctionalTests)]
    public sealed class ConcorrentArrayTests
    {
        [Theory]
        [MemberData(nameof(AddAndRetrieveData))]
        public void SimpleAddAndRetrieve(Entry<string, string> entry)
        {
            var concurrentArray = new ConcurrentArrayBuilder<string, string>().Build();

            var tryAddResult = concurrentArray.TryAdd(entry);
            var foundEntry = concurrentArray.Find(entry.HashCode, entry.Key);

            tryAddResult.OperationResult.Should().Be(AddResult.AddSuccessful);
            foundEntry.Should().BeSameAs(entry);
        }

        public static readonly TestData AddAndRetrieveData =
            new[]
            {
                new object[] { new Entry<string, string>(1, "Foo", "Bar") },
                new object[] { new Entry<string, string>(42, "Bar", "Baz") }
            };

        [Theory]
        [InlineData(31)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(143483)]
        public void InitialCapacity(int initialCapacity)
        {
            var concurrentArray = new ConcurrentArrayBuilder<string, object>().WithCapacity(initialCapacity)
                                                                              .Build();

            concurrentArray.Capacity.Should().Be(initialCapacity);
        }

        [Theory]
        [MemberData(nameof(AddTestData))]
        public void AddAndRetrieveAll(Entry<int, string>[] entries)
        {
            var concurrentArray = new ConcurrentArrayBuilder<int, string>().Build();
            foreach (var entry in entries)
            {
                concurrentArray.TryAdd(entry);
            }

            foreach (var entry in entries)
            {
                var foundEntry = concurrentArray.Find(entry.HashCode, entry.Key);
                foundEntry.Should().BeSameAs(entry);
            }
        }

        [Theory]
        [MemberData(nameof(AddTestData))]
        public void CountMustReflectAddedEntries(Entry<int, string>[] entries)
        {
            var concurrentArray = new ConcurrentArrayBuilder<int, string>().Build();
            foreach (var entry in entries)
            {
                concurrentArray.TryAdd(entry);
            }

            concurrentArray.Count.Should().Be(entries.Length);
        }

        public static readonly TestData AddTestData =
            new[]
            {
                new object[]
                {
                    new[]
                    {
                        new Entry<int, string>(42, 42, "Foo"),
                        new Entry<int, string>(12, 12, "Bar"),
                        new Entry<int, string>(88, 88, "Baz")
                    }
                },
                new object[]
                {
                    new[]
                    {
                        new Entry<int, string>(42, 42, "Foo"),
                        new Entry<int, string>(12, 12, "Bar"),
                        new Entry<int, string>(88, 88, "Baz"),
                        new Entry<int, string>(90, 90, "Qux"),
                        new Entry<int, string>(-116, -116, "Quux")
                    }
                }
            };

        [Fact]
        public void CountMustBeZeroAtBeginning()
        {
            var concurrentArray = new ConcurrentArrayBuilder<uint, string>().Build();

            concurrentArray.Count.Should().Be(0);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-13)]
        [InlineData(-10004)]
        public void InvalidCapacity(int capacity)
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new ConcurrentArray<string, object>(capacity);

            act.ShouldThrow<ArgumentOutOfRangeException>()
               .And.ParamName.Should().Be(nameof(capacity));
        }

        [Fact]
        public void CustomKeyComparer()
        {
            var keyComparerMock = new KeyComparerMock();
            var concurrentArray = new ConcurrentArrayBuilder<int, object>().WithKeyComparer(keyComparerMock)
                                                                           .Build();
            concurrentArray.TryAdd(new Entry<int, object>(6, 6, null));

            concurrentArray.Find(6, 6);

            keyComparerMock.EqualsMustHaveBeenCalledAtLeastOnce();
        }

        [Fact]
        public void KeyComparerNotNull()
        {
            Action act = () => new ConcurrentArrayBuilder<string, object>().WithKeyComparer(null)
                                                                           .Build();

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("keyComparer");
        }

        private sealed class KeyComparerMock : IEqualityComparer<int>
        {
            private int _equalsCallCount;

            public bool Equals(int x, int y)
            {
                ++_equalsCallCount;
                return x == y;
            }

            public int GetHashCode(int value)
            {
                return value;
            }

            public void EqualsMustHaveBeenCalledAtLeastOnce()
            {
                _equalsCallCount.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public void TryAddEntryExists()
        {
            var concurrentArray = new ConcurrentArrayBuilder<string, object>().Build();
            var existingEntry = new Entry<string, object>(42, "Foo", null);
            concurrentArray.TryAdd(existingEntry);

            var addInfo = concurrentArray.TryAdd(new Entry<string, object>(42, "Foo", "Bar"));

            addInfo.OperationResult.Should().Be(AddResult.ExistingEntryFound);
            addInfo.TargetEntry.Should().Be(existingEntry);
        }

        [Fact]
        public void TryAddWhenArrayIsFull()
        {
            var concurrentArray = new ConcurrentArrayBuilder<int, object>().WithCapacity(4)
                                                                           .Build()
                                                                           .FillArray();

            var addInfo = concurrentArray.TryAdd(new Entry<int, object>(5, 5, null));

            addInfo.OperationResult.Should().Be(AddResult.ArrayFull);
            addInfo.TargetEntry.Should().BeNull();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(19, false)]
        public void FindWhenArrayIsFull(int targetKey, bool shouldBeFound)
        {
            var concurrentArray = new ConcurrentArrayBuilder<int, object>().WithCapacity(3)
                                                                           .Build()
                                                                           .FillArray();

            var foundEntry = concurrentArray.Find(targetKey.GetHashCode(), targetKey);

            if (shouldBeFound)
                foundEntry.Key.Should().Be(targetKey);
            else
                foundEntry.Should().BeNull();
        }

        [Fact]
        public void FindWhenEmpty()
        {
            var concurrentArray = new ConcurrentArrayBuilder<int, object>().Build();

            var foundEntry = concurrentArray.Find(42, 42);

            foundEntry.Should().BeNull();
        }

        [Fact]
        public void ConcurrentAddTest()
        {
            var primeStrategy = new DoublingPrimeNumbersStrategy();
            var processorCount = Environment.ProcessorCount;
            var entryCount = processorCount * 100000;
            var allNumbers = Enumerable.Range(1, entryCount).ToArray();
            var groupsPerTask = allNumbers.GroupBy(number => number % processorCount)
                                          .ToArray();
            var concurrentArray = new ConcurrentArrayBuilder<int, object>().WithCapacity(primeStrategy.GetNextSize(entryCount))
                                                                           .Build();
            Parallel.ForEach(groupsPerTask, group =>
                                            {
                                                foreach (var number in group)
                                                {
                                                    var addResult = concurrentArray.TryAdd(new Entry<int, object>(number.GetHashCode(), number, null));
                                                    addResult.OperationResult.Should().Be(AddResult.AddSuccessful);
                                                }
                                            });

            concurrentArray.Count.Should().Be(allNumbers.Length);
            foreach (var number in allNumbers)
            {
                var entry = concurrentArray.Find(number.GetHashCode(), number);
                entry.Should().NotBeNull();
            }
        }

        [Fact]
        public void TryAddParameterNull()
        {
            var concurrentArray = new ConcurrentArrayBuilder<string, object>().Build();

            Action act = () => concurrentArray.TryAdd(null);

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("entry");
        }

        [Fact]
        public void FindKeyNull()
        {
            var concurrentArray = new ConcurrentArrayBuilder<string, object>().Build();

            Action act = () => concurrentArray.Find(42, null);

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("key");
        }
    }
}