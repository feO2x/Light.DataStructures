using FluentAssertions;
using Xunit;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.FunctionalTests)]
    public sealed class SieveOfEratosthenesTests
    {
        [Theory]
        [InlineData(10, new[] { 2, 3, 5, 7 })]
        [InlineData(45, new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43 })]
        [InlineData(71, new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71 })]
        [InlineData(103, new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103 })]
        [InlineData(106, new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103 })]
        public void GetAllPrimes(int targetNumber, int[] expectedPrimeNumbers)
        {
            var actual = SieveOfEratosthenes.GetAllPrimeNumbers(targetNumber);

            actual.Should().BeEquivalentTo(expectedPrimeNumbers);
        }
    }
}