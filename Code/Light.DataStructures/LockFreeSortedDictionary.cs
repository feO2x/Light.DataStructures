using System;
using System.Collections.Generic;
using System.Threading;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DataStructures
{
    public sealed class LockFreeSortedDictionary<TKey, TValue>
    {
        private readonly IEqualityComparer<TKey> _equalityComparer = EqualityComparer<TKey>.Default;
        private int _count;
        private Entry[] _internalArray;

        public LockFreeSortedDictionary()
        {
            _internalArray = new Entry[31];
        }

        public int Count => _count;

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
        }

        public LockFreeSortedDictionary<TKey, TValue> Add(TKey key, TValue value)
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

    }
}