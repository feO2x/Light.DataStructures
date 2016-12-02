using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public static class SieveOfEratosthenes
    {
        public static IEnumerable<int> GetAllPrimeNumbers(int targetNumber)
        {
            targetNumber.MustNotBeLessThan(2);

            var possiblePrimeNumbers = new bool[targetNumber + 1];
            var boundary = (int) Math.Ceiling(Math.Sqrt(targetNumber));

            for (var i = 2; i <= boundary; ++i)
            {
                if (possiblePrimeNumbers[i])
                    continue;

                for (var j = i * i; j <= targetNumber; j += i)
                {
                    possiblePrimeNumbers[j] = true;
                }
            }

            for (var i = 2; i < possiblePrimeNumbers.Length; i++)
            {
                if (possiblePrimeNumbers[i] == false)
                    yield return i;
            }
        }
    }
}