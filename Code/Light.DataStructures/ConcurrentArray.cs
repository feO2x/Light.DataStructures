namespace Light.DataStructures
{
    public class ConcurrentArray<TKey, TValue>
    {
        private Entry<TKey, TValue> _entry;
        private int _capacity;

        public ConcurrentArray()
        {
        }

        public ConcurrentArray(int capacity)
        {
            _capacity = capacity;
        }

        public int Capacity => _capacity;

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