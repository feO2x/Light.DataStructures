using System.Collections.Generic;

namespace Light.DataStructures.Tests
{
    public sealed class ConcurrentArrayBuilder<TKey, TValue>
    {
        private int _capacity = 31;
        private IEqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;

        public ConcurrentArrayBuilder<TKey, TValue> WithCapacity(int capacity)
        {
            _capacity = capacity;
            return this;
        }

        public ConcurrentArrayBuilder<TKey, TValue> WithKeyComparer(IEqualityComparer<TKey> keyComparer)
        {
            _keyComparer = keyComparer;
            return this;
        }

        public ConcurrentArray<TKey, TValue> Build()
        {
            return new ConcurrentArray<TKey, TValue>(_capacity, _keyComparer);
        }
    }
}