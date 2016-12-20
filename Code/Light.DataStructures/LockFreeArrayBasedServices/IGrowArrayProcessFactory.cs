namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public interface IGrowArrayProcessFactory<TKey, TValue>
    {
        GrowArrayProcess<TKey, TValue> CreateGrowArrayProcess(ConcurrentArray<TKey, TValue> oldArray, int newArraySize, ExchangeArray<TKey, TValue> setNewArray);
    }
}