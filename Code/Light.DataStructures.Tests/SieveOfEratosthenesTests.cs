using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Light.GuardClauses;
using Xunit;
using Xunit.Abstractions;

namespace Light.DataStructures.Tests
{
    public sealed class SieveOfEratosthenesTests
    {
        private readonly ITestOutputHelper _output;

        public SieveOfEratosthenesTests(ITestOutputHelper output)
        {
            _output = output;
        }

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

        [Fact(Skip = "Only used to determine prime numbers for DoublingPrimeNUmbersStrategy.")]
        public void PrintAllPrimesThatAreAtLeastTheDoulbeOfThePrevious()
        {
            const int startPrime = 31;
            //           Int32.Max = 2147483647
            const int targetNumber = 2100000000;
            var primes = SieveOfEratosthenes.GetAllPrimeNumbers(targetNumber)
                                            .SkipWhile(number => number < startPrime)
                                            .ToArray();
            var doubledPrimes = FilterDoublePrimes(primes);

            _output.WriteLine($"All primes from {startPrime} up to {targetNumber} that are at least double the previous prime.");
            foreach (var doubledPrime in doubledPrimes)
            {
                _output.WriteLine($"{doubledPrime},");
            }
        }

        private static IEnumerable<int> FilterDoublePrimes(int[] primeNumbers)
        {
            primeNumbers.MustNotBeNullOrEmpty();

            var currentPrime = primeNumbers[0];
            yield return currentPrime;

            var newDouble = currentPrime * 2L;
            for (var i = 1; i < primeNumbers.Length; ++i)
            {
                currentPrime = primeNumbers[i];
                if (currentPrime < newDouble)
                    continue;

                yield return currentPrime;

                newDouble = currentPrime * 2L;
            }
        }
    }
}