using FluentAssertions;
using Xunit;

namespace Light.DataStructures.Tests
{
    public sealed class LockFreeArrayBasedDictionaryTests
    {
        [Fact]
        public void KickOff()
        {
            var dictionary = new LockFreeSortedDictionary<string, string>();

            dictionary.Add("Foo", "Bar");

            dictionary["Foo"].Should().Be("Bar");
        }
    }
}