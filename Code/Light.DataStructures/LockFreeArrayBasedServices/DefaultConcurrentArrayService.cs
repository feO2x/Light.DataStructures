using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public class DefaultConcurrentArrayService<TKey, TValue> : IConcurrentArrayService<TKey, TValue>
    {
        public readonly IGrowArrayStrategy GrowArrayStrategy = new DoublingPrimeNumbersStrategy();
        public const float DefaultLoadThreshhold = 0.5f;
        public readonly float LoadThreshhold = DefaultLoadThreshhold;
        
        public ConcurrentArray<TKey, TValue> CreateInitial(IEqualityComparer<TKey> keyComparer)
        {
            return new ConcurrentArray<TKey, TValue>(GrowArrayStrategy.GetInitialSize(), keyComparer);
        }

        public IGrowArrayProcess<TKey, TValue> CreateGrowProcessIfNecessary(ConcurrentArray<TKey, TValue> currentArray, Action<ConcurrentArray<TKey, TValue>> setNewArray)
        {
            currentArray.MustNotBeNull(nameof(currentArray));

            var currentLoad = (float) currentArray.Count / currentArray.Capacity;
            return currentLoad > LoadThreshhold ? new DefaultGrowArrayProcess<TKey, TValue>(currentArray, GrowArrayStrategy.GetNextSize(currentArray.Capacity), setNewArray) : null;
        }
    }
}