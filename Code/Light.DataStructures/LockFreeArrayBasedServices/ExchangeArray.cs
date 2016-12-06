namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public delegate void ExchangeArray<TKey, TValue>(ConcurrentArray<TKey, TValue> oldArray, ConcurrentArray<TKey, TValue> newArray);
}