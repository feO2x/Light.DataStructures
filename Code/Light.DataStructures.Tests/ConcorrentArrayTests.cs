using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DataStructures.Tests
{
    public sealed class ConcorrentArrayTests
    {
        [Theory]
        [MemberData(nameof(AddAndRetrieveData))]
        public void AddAndRetrieve(Entry<string, string> entry)
        {
            var concurrentArray = new ConcurrentArray<string, string>();

            var tryAddResult = concurrentArray.TryAdd(entry);
            var foundEntry = concurrentArray.Find(entry.HashCode, entry.Key);

            tryAddResult.Should().BeTrue();
            foundEntry.Should().BeSameAs(entry);
        }

        public static readonly TestData AddAndRetrieveData =
            new[]
            {
                new object[] { new Entry<string, string>(1, "Foo", "Bar") },
                new object[] { new Entry<string, string>(42, "Bar", "Baz") }
            };
    }
}