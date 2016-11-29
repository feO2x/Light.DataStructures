namespace Light.DataStructures
{
    public class ConcurrentArray<TKey, TValue>
    {
        public const int DefaultCapacity = 31;
        private readonly int _capacity;
        private Entry<TKey, TValue> _entry;

        public ConcurrentArray() : this (DefaultCapacity) { }

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