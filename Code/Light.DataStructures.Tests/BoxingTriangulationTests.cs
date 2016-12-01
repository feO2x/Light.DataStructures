using System.Threading;
using FluentAssertions;
using Xunit;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.ThirdPartyTriangulation)]
    public class BoxingTriangulationTests
    {
        private object _value;

        [Theory]
        [InlineData(1)]
        [InlineData(true)]
        public void BoxedValueEqualityWithInterlocked<T>(T value)
        {
            object boxedValue1 = value;
            object boxedValue2 = value;

            var previousValue = Interlocked.CompareExchange(ref _value, boxedValue1, null);
            previousValue.Should().BeNull();

            previousValue = Interlocked.CompareExchange(ref _value, boxedValue2, value);

            previousValue.Should().Be(value);
            _value.Should().BeSameAs(boxedValue1);
        }

        [Fact]
        public void EqualityWhenBoxing()
        {
            var number = 42;
            object boxedValue1 = number;
            object boxedValue2 = number;

            boxedValue1.Should().NotBeSameAs(boxedValue2);
            number.Equals(boxedValue1).Should().BeTrue();
            boxedValue2.Equals(number).Should().BeTrue();
            boxedValue1.Equals(boxedValue2).Should().BeTrue();
        }
    }
}