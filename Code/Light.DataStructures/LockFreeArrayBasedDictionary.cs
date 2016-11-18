using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Light.DataStructures
{
    public class LockFreeArrayBasedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        // TODO: the comparers should be configurable via the constructor
        private readonly IEqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;
        private readonly IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;
        private int _count;
        private Entry[] _internalArray;

        public LockFreeArrayBasedDictionary()
        {
            _internalArray = new Entry[31];
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var hashCode = _keyComparer.GetHashCode(item.Key);
            var targetIndex = GetTargetBucketIndex(hashCode);

            while (true)
            {
                var targetEntry = _internalArray[targetIndex];
                if (targetEntry == null)
                    return false;

                if (targetEntry.HashCode == hashCode &&
                    _keyComparer.Equals(item.Key, targetEntry.Key) &&
                    _valueComparer.Equals(item.Value, targetEntry.Value))
                    return true;

                targetIndex = (targetIndex + 1) % _internalArray.Length;
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count => _count;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            var hashCode = _keyComparer.GetHashCode(key);
            var targetIndex = GetTargetBucketIndex(hashCode);

            while (true)
            {
                var targetEntry = _internalArray[targetIndex];
                if (targetEntry == null)
                    return false;
                if (hashCode == targetEntry.HashCode && _keyComparer.Equals(key, targetEntry.Key))
                    return true;

                targetIndex = (targetIndex + 1) % _internalArray.Length;
            }
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys { get; }
        ICollection<TValue> IDictionary<TKey, TValue>.Values { get; }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                var hashCode = _keyComparer.GetHashCode(key);
                var targetIndex = GetTargetBucketIndex(hashCode);

                while (true)
                {
                    var targetEntry = _internalArray[targetIndex];
                    if (targetEntry == null)
                        throw new ArgumentException($"There is no entry with key \"{key}\"", nameof(key));
                    if (hashCode == targetEntry.HashCode && _keyComparer.Equals(key, targetEntry.Key))
                        return targetEntry.Value;

                    targetIndex = (targetIndex + 1) % _internalArray.Length;
                }
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                var hashCode = _keyComparer.GetHashCode(key);
                var newEntry = new Entry(hashCode, key, value);
                var targetIndex = GetTargetBucketIndex(hashCode);

                while (true)
                {
                    var targetEntry = _internalArray[targetIndex];
                    if (targetEntry == null)
                    {
                        if (Interlocked.CompareExchange(ref _internalArray[targetIndex], newEntry, null) == null)
                            return;

                        goto UpdateIndex;
                    }
                    if (hashCode == targetEntry.HashCode && _keyComparer.Equals(key, targetEntry.Key))
                    {
                        if (Interlocked.CompareExchange(ref _internalArray[targetIndex], newEntry, targetEntry) == targetEntry)
                            return;

                        throw new InvalidOperationException($"Could not update entry with key {key} because another thread performed this action.");
                    }

                    UpdateIndex:
                    targetIndex = (targetIndex + 1) % _internalArray.Length;
                }
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(Volatile.Read(ref _internalArray));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>) this).GetEnumerator();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            Clear();
        }

        public LockFreeArrayBasedDictionary<TKey, TValue> Add(KeyValuePair<TKey, TValue> item)
        {
            return Add(item.Key, item.Value);
        }

        public bool Clear()
        {
            var newArray = new Entry[31];
            var currentArray = Volatile.Read(ref _internalArray);
            if (Interlocked.CompareExchange(ref _internalArray, newArray, currentArray) != currentArray) return false;

            Interlocked.Exchange(ref _count, 0);
            return true;
        }

        public LockFreeArrayBasedDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            var hashCode = _keyComparer.GetHashCode(key);
            var targetIndex = GetTargetBucketIndex(hashCode); // TODO: how do I know which target I should use? What if the array size changes?
            var entry = new Entry(hashCode, key, value);

            while (true)
            {
                if (_internalArray[targetIndex] == null &&
                    Interlocked.CompareExchange(ref _internalArray[targetIndex], entry, null) == null)
                {
                    Interlocked.Increment(ref _count);
                    return this;
                }

                targetIndex = (targetIndex + 1) % _internalArray.Length;
            }
        }

        private int GetTargetBucketIndex(int hashCode)
        {
            return Math.Abs(hashCode) % _internalArray.Length;
        }

        private sealed class Entry
        {
            public readonly int HashCode;
            public readonly TKey Key;
            public readonly TValue Value;

            public Entry(int hashCode, TKey key, TValue value)
            {
                HashCode = hashCode;
                Key = key;
                Value = value;
            }
        }

        private class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly Entry[] _array;
            private KeyValuePair<TKey, TValue> _current;
            private int _currentIndex;

            public Enumerator(Entry[] array)
            {
                _array = array;
                _currentIndex = -1;
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (_currentIndex + 1 == _array.Length)
                        return false;

                    var targetNode = Volatile.Read(ref _array[++_currentIndex]);
                    if (targetNode == null)
                        continue;

                    _current = new KeyValuePair<TKey, TValue>(targetNode.Key, targetNode.Value);
                    return true;
                }
            }

            public void Reset()
            {
                _current = default(KeyValuePair<TKey, TValue>);
                _currentIndex = -1;
            }

            public KeyValuePair<TKey, TValue> Current => _current;

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
    }
}