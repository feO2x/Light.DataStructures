using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Light.DataStructures
{
    public class LockFreeArrayBasedDictionary<TKey, TValue> : IConcurrentDictionary<TKey, TValue>, IDictionary<TKey, TValue>
    {
        // TODO: the comparers should be configurable via the constructor
        private readonly IEqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;
        private readonly IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;
        private int _count;
        private float _loadThreshold = DefaultLoadThreshold;
        private Entry[] _internalArray;
        private Entry[] _newArray;
        private Task _copyTask;
        private const float DefaultLoadThreshold = 0.7f;
        private readonly IGrowArrayStrategy _growArrayStrategy = new DoublingPrimeNumbersStrategy();
        
        public LockFreeArrayBasedDictionary()
        {
            _internalArray = new Entry[_growArrayStrategy.GetInitialSize()];
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

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            throw new NotImplementedException();
        }

        public bool GetOrAdd(TKey key, Func<TValue> createValue, out TValue value)
        {
            throw new NotImplementedException();
        }

        public TValue GetOrAdd(TKey key, Func<TValue> createValue)
        {
            throw new NotImplementedException();
        }

        public bool AddOrUpdate(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool TryRemove(TKey key, out TValue value)
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
            var newArray = new Entry[_growArrayStrategy.GetInitialSize()];
            var currentArray = Volatile.Read(ref _internalArray);
            if (Interlocked.CompareExchange(ref _internalArray, newArray, currentArray) != currentArray)
                return false;

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
                if (Interlocked.CompareExchange(ref targetArray[targetIndex], entry, null) == null)
                    return IncrementCount();    // TODO: if a new array is created here, we might want to copy the entry to it, too

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
            if (Volatile.Read(ref _copyTask) != null)
                return this;

            // Check if the number of elements exceeds the load threshold of the current array
            var array = Volatile.Read(ref _internalArray);
            if ((float) newCount / array.Length < Volatile.Read(ref _loadThreshold))
                return this;

            // If yes, then create a task that will copy all elements from the old array to the new one.
            var copyTask = new Task(CopyFromOldToNewArray);
            if (Interlocked.CompareExchange(ref _copyTask, copyTask, null) != null) // If _copyTask was not null, then another thread created the task already
                return this;

            var newArray = new Entry[_growArrayStrategy.GetNextSize(array.Length)];
            Volatile.Write(ref _newArray, newArray);
            copyTask.Start();
            return this;
        }

        private void CopyFromOldToNewArray()
        {
            // Read both old array and new array
            var oldArray = Volatile.Read(ref _internalArray);
            var newArray = Volatile.Read(ref _newArray);

            // Run through the old array and copy every entry to the new one
            for (var i = 0; i < oldArray.Length; ++i)
            {
                var entry = Volatile.Read(ref oldArray[i]);
                // If there is no entry, then go to the next one
                if (entry == null)
                    continue;

                // Insert the entry in the new array
                var targetIndex = GetTargetBucketIndex(entry.HashCode, newArray.Length);
                while (true)
                {
                    if (Interlocked.CompareExchange(ref newArray[targetIndex], entry, null) == null)
                        break;

                    targetIndex = IncrementTargetIndex(targetIndex, newArray.Length);
                }
            }
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

                    var currentEntry = Volatile.Read(ref _array[++_currentIndex]);
                    if (currentEntry == null)
                        continue;

                    _current = new KeyValuePair<TKey, TValue>(currentEntry.Key, currentEntry.Value);
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