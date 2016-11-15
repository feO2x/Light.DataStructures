using System.Threading;
using FluentAssertions;
using Xunit;

namespace Light.DataStructures.Tests
{
    public sealed class LockFreeArrayBasedDictionaryTests
    {
        [Fact]
        public void KickOff()
        {
            var dictioanry = new LockFreeSortedDictionary<string, string>();

            dictioanry.Add("Foo", "Bar");

            dictioanry["Foo"].Should().Be("Bar");
        }
    }
}