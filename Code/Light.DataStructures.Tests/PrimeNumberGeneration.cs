using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Xunit;
using Xunit.Abstractions;

namespace Light.DataStructures.Tests
{
    public sealed class PrimeNumberGeneration
    {
        private readonly ITestOutputHelper _output;

        public PrimeNumberGeneration(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait("Category", Traits.CreationScripts)]
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