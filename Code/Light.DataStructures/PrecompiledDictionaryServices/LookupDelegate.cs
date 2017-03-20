namespace Light.DataStructures.PrecompiledDictionaryServices
{
    public delegate bool LookupDelegate<in TKey, TValue>(TKey key, out TValue value);
}