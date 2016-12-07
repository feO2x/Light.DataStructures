using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public class DefaultConcurrentArrayService<TKey, TValue> : IConcurrentArrayService<TKey, TValue>
    {
        public const float DefaultLoadThreshold = 0.5f;
        public readonly IGrowArrayStrategy GrowArrayStrategy = new DoublingPrimeNumbersStrategy();
        public readonly float LoadThreshold = DefaultLoadThreshold;

        public ConcurrentArray<TKey, TValue> CreateInitial(IEqualityComparer<TKey> keyComparer)
        {
            return new ConcurrentArray<TKey, TValue>(GrowArrayStrategy.GetInitialSize(), keyComparer);
        }

        public IGrowArrayProcess<TKey, TValue> CreateGrowProcessIfNecessary(ConcurrentArray<TKey, TValue> currentArray, ExchangeArray<TKey, TValue> exchangeArray)
        {
            currentArray.MustNotBeNull(nameof(currentArray));

            var currentLoad = (float) currentArray.Count / currentArray.Capacity;
            return currentLoad > LoadThreshold ? new DefaultGrowArrayProcess<TKey, TValue>(currentArray, GrowArrayStrategy.GetNextSize(currentArray.Capacity), exchangeArray) : null;
        }
    }
}