using System;
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
            var concurrentArray = CreateDefaultConcurrentArray<string, string>();

            var tryAddResult = concurrentArray.TryAdd(entry);
            var foundEntry = concurrentArray.Find(entry.HashCode, entry.Key);

            tryAddResult.OperationResult.Should().Be(AddResult.Successful);
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
            var concurrentArray = new ConcurrentArray<string, object>(initialCapacity);

            concurrentArray.Capacity.Should().Be(initialCapacity);
        }

        [Theory]
        [MemberData(nameof(AddAndRetrieveAllData))]
        public void AddAndRetrieveAll(Entry<int, string>[] entries)
        {
            var concurrentArray = CreateDefaultConcurrentArray<int, string>();
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
                    new []
                    {
                        new Entry<int, string>(42, 42, "Foo"),
                        new Entry<int, string>(12, 12, "Bar"),
                        new Entry<int, string>(88, 88, "Baz")
                    }
                },
                new object[]
                {
                    new []
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

        private static ConcurrentArray<TKey, TValue> CreateDefaultConcurrentArray<TKey, TValue>()
        {
            return new ConcurrentArray<TKey, TValue>(31);
        }
    }
}