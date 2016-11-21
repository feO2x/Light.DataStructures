using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Light.DataStructures
{
    public class LockFreeArrayBasedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        // TODO: the comparers should be configurable via the constructor
        private readonly IEqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;
        private readonly IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;
        private int _count;
        private float _loadThreshold = DefaultLoadThreshold;
        private Entry[] _internalArray;
        private Entry[] _newArray;
        private Task _increaseCapacityTask;
        private const float DefaultLoadThreshold = 0.7f;
        
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
            var targetArray = GetTargetArray();
            var targetIndex = GetTargetBucketIndex(hashCode, targetArray.Length);

            while (true)
            {
                var targetEntry = Volatile.Read(ref targetArray[targetIndex]);
                if (targetEntry == null)
                    return false;

                if (targetEntry.HashCode == hashCode &&
                    _keyComparer.Equals(item.Key, targetEntry.Key) &&
                    _valueComparer.Equals(item.Value, targetEntry.Value))
                    return true;

                targetIndex = IncrementTargetIndex(targetIndex, targetArray.Length);
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
            var targetArray = GetTargetArray();
            var targetIndex = GetTargetBucketIndex(hashCode, targetArray.Length);

            while (true)
            {
                var targetEntry = Volatile.Read(ref targetArray[targetIndex]);
                if (targetEntry == null)
                    return false;
                if (hashCode == targetEntry.HashCode && _keyComparer.Equals(key, targetEntry.Key))
                    return true;

                // TODO: we have to search both arrays if possible
                targetIndex = IncrementTargetIndex(targetIndex, targetArray.Length);
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

        public float LoadThreshold
        {
            get { return _loadThreshold; }
            set
            {
                value.MustNotBeLessThan(0f);
                value.MustNotBeGreaterThan(1f);
                _loadThreshold = value;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                var hashCode = _keyComparer.GetHashCode(key);
                var targetArray = GetTargetArray();
                var targetIndex = GetTargetBucketIndex(hashCode, targetArray.Length);

                while (true)
                {
                    var targetEntry = _internalArray[targetIndex];
                    if (targetEntry == null)
                        throw new KeyNotFoundException($"There is no entry with key \"{key}\"");
                    if (hashCode == targetEntry.HashCode && _keyComparer.Equals(key, targetEntry.Key))
                        return targetEntry.Value;

                    targetIndex = IncrementTargetIndex(targetIndex, targetArray.Length);
                }
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                var hashCode = _keyComparer.GetHashCode(key);
                var newEntry = new Entry(hashCode, key, value);
                var targetArray = GetTargetArray();
                var targetIndex = GetTargetBucketIndex(hashCode, targetArray.Length);

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
            var entry = new Entry(hashCode, key, value);

            var targetArray = GetTargetArray();
            var targetIndex = GetTargetBucketIndex(hashCode, targetArray.Length);
            while (true)
            {
                if (targetArray[targetIndex] == null &&
                    Interlocked.CompareExchange(ref targetArray[targetIndex], entry, null) == null)
                        return IncrementCount();

                var previousArray = targetArray;
                targetArray = GetTargetArray();
                if (previousArray == targetArray)
                    targetIndex = (targetIndex + 1) % targetArray.Length;
                else
                    targetIndex = GetTargetBucketIndex(hashCode, targetArray.Length);
            }
        }

        private LockFreeArrayBasedDictionary<TKey, TValue> IncrementCount()
        {
            // Increment the count
            var newCount = Interlocked.Increment(ref _count);
            
            // Check if there is already an Increase Capacity Task active. If yes, then we do not need to check the load threshold
            if (Volatile.Read(ref _increaseCapacityTask) != null)
                return this;

            // Check if the number of elements exceeds the load threshold of the current array
            var array = Volatile.Read(ref _internalArray);
            if ((float) newCount / array.Length < Volatile.Read(ref _loadThreshold))
                return this;

            // If yes, then create a task that will create a new array, copy all elements from the old to the new one and replace it.
            var increaseCapacityTask = new Task(IncreaseCapacity);
            if (Interlocked.CompareExchange(ref _increaseCapacityTask, increaseCapacityTask, null) != null) // If _increaseCapacityTask was not null, then another thread created the task already
                return this;

            _newArray = new Entry[67]; // TODO: exchange this with a proper algorithm to fetch the next prime number
            increaseCapacityTask.Start();
            return this;
        }

        private void IncreaseCapacity()
        {

        }

        private Entry[] GetTargetArray()
        {
            return Volatile.Read(ref _newArray) ?? Volatile.Read(ref _internalArray);
        }

        private static int GetTargetBucketIndex(int hashCode, int arrayLength)
        {
            return Math.Abs(hashCode) % arrayLength;
        }

        private static int IncrementTargetIndex(int targetIndex, int arrayLenght)
        {
            return (targetIndex + 1) % arrayLenght;
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