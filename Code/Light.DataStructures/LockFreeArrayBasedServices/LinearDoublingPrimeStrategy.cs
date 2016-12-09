using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public sealed class LinearDoublingPrimeStrategy : IGrowArrayStrategy
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

        private readonly int _reprobingThreshold;
        private readonly float _loadThreshold;

        public LinearDoublingPrimeStrategy(int reprobingThreshold = 20, float loadThreshold = 0.5f)
        {
            reprobingThreshold.MustNotBeLessThan(0, nameof(reprobingThreshold));
            loadThreshold.MustNotBeLessThan(0f, nameof(loadThreshold));
            loadThreshold.MustNotBeGreaterThan(1f, nameof(loadThreshold));

            _reprobingThreshold = reprobingThreshold;
            _loadThreshold = loadThreshold;
        }


        public int GetInitialSize()
        {
            return DoublingPrimeNumbers[0];
        }

        public int? GetNextCapacity(int currentCount, int currentCapacity, int reprobingCount)
        {
            var currentLoad = (float) currentCount / currentCapacity;
            if (currentLoad < _loadThreshold && reprobingCount < _reprobingThreshold)
                return null;

            return GetNextCapacity(currentCapacity);
        }

        public static int GetNextCapacity(int currentSize)
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