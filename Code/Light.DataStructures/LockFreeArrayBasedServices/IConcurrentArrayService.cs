using System.Collections.Generic;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public interface IConcurrentArrayService<TKey, TValue>
    {
        ConcurrentArray<TKey, TValue> CreateInitial(IEqualityComparer<TKey> keyComparer);
        GrowArrayProcess<TKey, TValue> CreateGrowProcessIfNecessary(ConcurrentArray<TKey, TValue> currentArray, ExchangeArray<TKey, TValue> exchangeArray);
    }
}