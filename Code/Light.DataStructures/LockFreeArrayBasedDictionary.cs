using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Light.GuardClauses.FrameworkExtensions;

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

        public int Count => _count;
        public bool IsReadOnly => false;

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

        public ICollection<TKey> Keys { get; }
        public ICollection<TValue> Values { get; }

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

                throw new NotImplementedException();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        private sealed class Entry : IEquatable<Entry>
        {
            private readonly int _entryHashCode;
            public readonly int HashCode;
            public readonly TKey Key;
            public readonly TValue Value;

            public Entry(int hashCode, TKey key, TValue value)
            {
                HashCode = hashCode;
                Key = key;
                Value = value;
                _entryHashCode = Equality.CreateHashCode(HashCode, Key, Value);
            }

            public bool Equals(Entry other)
            {
                if (other == null)
                    return false;

                return HashCode == other.HashCode &&
                       Key.Equals(other.Key) &&
                       Value.Equals(other.Value);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Entry);
            }

            public override int GetHashCode()
            {
                return _entryHashCode;
            }
        }
    }
}