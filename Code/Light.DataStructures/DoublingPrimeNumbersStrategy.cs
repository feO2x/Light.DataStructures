using System;
using System.Collections.Generic;

namespace Light.DataStructures
{
    public interface IGrowArrayStrategy
    {
        int GetInitialSize();
        int GetNextSize(int currentSize);
    }

    public sealed class DoublingPrimeNumbersStrategy : IGrowArrayStrategy
    {
        public static readonly IReadOnlyList<int> DoublingPrimeNumbers =
            new[]
            {
                31,
                67,
                137,
                277,
                557,
                1117,
                2237,
                4481,
                8963,
                17929,
                35863,
                71741,
                143483,
                286973,
                573953,
                1147921,
                2295859,
                4591721,
                9183457,
                18366923,
                36733847,
                73467739,
                146935499,
                293871013,
                587742049,
                1175484103
            };

        public int GetInitialSize()
        {
            return DoublingPrimeNumbers[0];
        }

        public int GetNextSize(int currentSize)
        {
            for (var i = 0; i < DoublingPrimeNumbers.Count; i++)
            {
                var prime = DoublingPrimeNumbers[i];
                if (prime <= currentSize)
                    continue;

                return prime;
            }

            throw new InvalidOperationException($"There is no prime number greater than {currentSize} configured.");
        }
    }
}