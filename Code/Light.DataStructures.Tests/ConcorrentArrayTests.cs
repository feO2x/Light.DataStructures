using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DataStructures.Tests
{
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
        [MemberData(nameof(AddAndRetrieveAllData))]
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

        public static readonly TestData AddAndRetrieveAllData =
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
        public void ArrayFull()
        {
            var concurrentArray = new ConcurrentArrayBuilder<int, object>().WithCapacity(4)
                                                                           .Build();
            for (var i = 0; i < 4; i++)
            {
                concurrentArray.TryAdd(new Entry<int, object>(i, i, null));
            }

            var addInfo = concurrentArray.TryAdd(new Entry<int, object>(5, 5, null));

            addInfo.OperationResult.Should().Be(AddResult.ArrayFull);
            addInfo.TargetEntry.Should().BeNull();
        }
    }
}