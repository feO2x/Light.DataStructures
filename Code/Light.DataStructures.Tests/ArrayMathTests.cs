using FluentAssertions;
using Light.DataStructures.LockFreeArrayBasedServices;
using Xunit;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.FunctionalTests)]
    public sealed class ArrayMathTests
    {
        [Theory]
        [InlineData(0, 0, 3, 0)]
        [InlineData(0, 1, 5, 1)]
        [InlineData(1, 6, 7, 5)]
        [InlineData(9, 0, 10, 1)]
        [InlineData(8, 1, 10, 3)]
        public void CalculateNumberOfSlotsBetween(int startIndex, int endIndex, int arrayLength, int expectedNumberOfSlots)
        {
            var actualNumberOfSlots = ArrayMath.CalculateNumberOfSlotsBetween(startIndex, endIndex, arrayLength);

            actualNumberOfSlots.Should().Be(expectedNumberOfSlots);
        }
    }
}