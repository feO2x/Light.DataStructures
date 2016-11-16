using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DataStructures.Tests
{
    public sealed class LockFreeArrayBasedDictionaryTests
    {
        [Fact]
        public void KickOff()
        {
            var dictionary = new LockFreeArrayBasedDictionary<string, string>();

            dictionary.Add("Foo", "Bar");

            dictionary["Foo"].Should().Be("Bar");
        }

        [Fact]
        public void ImplementsIDictionary()
        {
            typeof(LockFreeArrayBasedDictionary<string, object>).Should().Implement<IDictionary<string, object>>();
        }
    }
}