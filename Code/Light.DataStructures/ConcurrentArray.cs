namespace Light.DataStructures
{
    public class ConcurrentArray<TKey, TValue>
    {
        private Entry<TKey, TValue> _entry;

        public bool TryAdd(Entry<TKey, TValue> entry)
        {
            _entry = entry;
            return true;
        }

        public Entry<TKey, TValue> Find(int entryHashCode, string entryKey)
        {
            return _entry;
        }
    }
}