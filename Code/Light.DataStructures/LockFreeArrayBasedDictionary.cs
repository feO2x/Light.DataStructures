using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DataStructures
{
    public sealed class LockFreeArrayBasedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IEqualityComparer<TKey> _equalityComparer = EqualityComparer<TKey>.Default;
        private int _count;
        private Entry[] _internalArray;

        public LockFreeArrayBasedDictionary()
        {
            _internalArray = new Entry[31];
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool ContainsKey(TKey key)
        {
            var hashCode = _equalityComparer.GetHashCode(key);
            var targetIndex = GetTargetBucketIndex(hashCode);

            while (true)
            {
                var targetEntry = _internalArray[targetIndex];
                if (targetEntry == null)
                    return false;
                if (hashCode == targetEntry.HashCode && _equalityComparer.Equals(key, targetEntry.Key))
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
                var hashCode = _equalityComparer.GetHashCode(key);
                var targetIndex = GetTargetBucketIndex(hashCode);

                while (true)
                {
                    var targetEntry = _internalArray[targetIndex];
                    if (targetEntry == null)
                        throw new ArgumentException($"There is no entry with key \"{key}\"", nameof(key));
                    if (hashCode == targetEntry.HashCode && _equalityComparer.Equals(key, targetEntry.Key))
                        return targetEntry.Value;

                    targetIndex = (targetIndex + 1) % _internalArray.Length;
                }
            }
            set
            {
                
            }
        }

        public LockFreeArrayBasedDictionary <TKey, TValue> Add(TKey key, TValue value)
        {
            var hashCode = _equalityComparer.GetHashCode(key);
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
            public readonly int HashCode;
            public readonly TKey Key;
            public readonly TValue Value;
            private readonly int _entryHashCode;

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

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}