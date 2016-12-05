using System;
using System.Collections.Generic;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public interface IConcurrentArrayService<TKey, TValue>
    {
        ConcurrentArray<TKey, TValue> CreateInitial(IEqualityComparer<TKey> keyComparer);
        IGrowArrayProcess<TKey, TValue> CreateGrowProcessIfNecessary(ConcurrentArray<TKey, TValue> currentArray, Action<ConcurrentArray<TKey, TValue>> setNewArray);
    }
}