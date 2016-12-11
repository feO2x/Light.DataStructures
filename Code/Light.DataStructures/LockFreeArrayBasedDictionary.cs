using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Light.DataStructures.LockFreeArrayBasedServices;
using Light.GuardClauses;

namespace Light.DataStructures
{
    public class LockFreeArrayBasedDictionary<TKey, TValue> : IConcurrentDictionary<TKey, TValue>, IDictionary<TKey, TValue>
    {
        private readonly IGrowArrayStrategy _growArrayStrategy;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly ExchangeArray<TKey, TValue> _setNewArray;
        private readonly IEqualityComparer<TValue> _valueComparer;
        private int _count;
        private ConcurrentArray<TKey, TValue> _currentArray;

        public LockFreeArrayBasedDictionary() : this(Options.Default) { }

        public LockFreeArrayBasedDictionary(Options options)
        {
            _keyComparer = options.KeyComparer;
            _valueComparer = options.ValueComparer;
            _growArrayStrategy = options.GrowArrayStrategy;
            _currentArray = CreateInitialArray();
            _setNewArray = SetNewArray;
        }

        public bool ContainsKey(TKey key)
        {
            var targetEntry = FindEntry(_keyComparer.GetHashCode(key), key);
            if (targetEntry == null)
                return false;

            var value = targetEntry.ReadValueVolatile();
            return value != Entry.Tombstone;
        }

        public bool TryUpdate(TKey key, TValue newValue)
        {
            var targetEntry = FindEntry(_keyComparer.GetHashCode(key), key);
            return targetEntry != null && targetEntry.TryUpdateValue(newValue).WasUpdateSuccessful;
        }

        public bool GetOrAdd(TKey key, Func<TValue> createValue, out TValue value)
        {
            var hashCode = _keyComparer.GetHashCode(key);

            // Find the target entry
            var targetEntry = FindEntry(hashCode, key);
            // If there is one, then try to read its current value
            if (targetEntry != null)
                return GetOrAddOnExistingEntry(targetEntry, createValue, out value);

            // Else try to add a new entry
            var createdValue = createValue();
            var addInfo = TryAddinternal(new Entry<TKey, TValue>(hashCode, key, createdValue));

            // If an entry was found during the add operation, try to get its value or update it when it is a tomb stone
            if (addInfo.OperationResult == AddResult.ExistingEntryFound)
                return GetOrAddOnExistingEntry(addInfo.TargetEntry, () => createdValue, out value);

            // Else the add operation was performed successfully
            value = createdValue;
            return true;
        }

        public TValue GetOrAdd(TKey key, Func<TValue> createValue)
        {
            TValue returnValue;
            GetOrAdd(key, createValue, out returnValue);
            return returnValue;
        }

        public bool AddOrUpdate(TKey key, TValue value)
        {
            var addResult = TryAddinternal(new Entry<TKey, TValue>(_keyComparer.GetHashCode(key), key, value));
            if (addResult.OperationResult == AddResult.AddSuccessful)
                return true;

            addResult.TargetEntry.TryUpdateValue(value);
            return false;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return TryAddinternal(new Entry<TKey, TValue>(_keyComparer.GetHashCode(key), key, value)).OperationResult == AddResult.AddSuccessful;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            var foundEntry = FindEntry(_keyComparer.GetHashCode(key), key);
            if (foundEntry == null)
                goto RemoveFailed;

            var readValue = foundEntry.ReadValueVolatile();
            if (readValue == Entry.Tombstone)
                goto RemoveFailed;

            if (foundEntry.TryMarkAsRemoved().WasUpdateSuccessful == false)
                goto RemoveFailed;

            Interlocked.Decrement(ref _count);
            value = (TValue) readValue;
            return true;

            RemoveFailed:
            value = default(TValue);
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var foundEntry = FindEntry(_keyComparer.GetHashCode(key), key);
            if (foundEntry == null)
            {
                value = default(TValue);
                return false;
            }

            var readValue = foundEntry.ReadValueVolatile();
            if (readValue == Entry.Tombstone)
            {
                value = default(TValue);
                return false;
            }

            value = (TValue) readValue;
            return true;
        }

        public bool Clear()
        {
            Volatile.Read(ref _currentArray).ReadGrowArrayProcessVolatile()?.Abort();
            var emptyArray = CreateInitialArray();
            var currentArray = Volatile.Read(ref _currentArray);
            if (Interlocked.CompareExchange(ref _currentArray, emptyArray, currentArray) != currentArray)
                return false;

            Interlocked.Exchange(ref _count, 0);
            return true;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            TryAdd(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            var hashCode = _keyComparer.GetHashCode(item.Key);
            var targetEntry = FindEntry(hashCode, item.Key);
            return targetEntry?.IsValueEqualTo(item.Value, _valueComparer) ?? false;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            array.MustNotBeNull(nameof(array));
            arrayIndex.MustNotBeLessThan(0, nameof(arrayIndex));

            foreach (var keyValuePair in this)
            {
                if (arrayIndex == array.Length)
                    throw new ArgumentException("There is not enough space to copy all elements to the target array.");
                array[arrayIndex++] = keyValuePair;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            var foundEntry = FindEntry(_keyComparer.GetHashCode(item.Key), item.Key);
            if (foundEntry == null)
                return false;

            var value = foundEntry.ReadValueVolatile();
            if (value == Entry.Tombstone || _valueComparer.Equals(item.Value, (TValue) value) == false)
                return false;

            var updateInfo = foundEntry.TryMarkAsRemoved();
            if (updateInfo.WasUpdateSuccessful == false)
                return false;

            Interlocked.Decrement(ref _count);
            return true;
        }

        public int Count => Volatile.Read(ref _count);

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            TryAdd(key, value);
        }

        public bool Remove(TKey key)
        {
            TValue value;
            return TryRemove(key, out value);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                var keys = new List<TKey>();
                foreach (var entry in this)
                {
                    keys.Add(entry.Key);
                }
                return keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                var values = new List<TValue>(_currentArray.Capacity);
                foreach (var entry in this)
                {
                    values.Add(entry.Value);
                }
                return values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                var hashCode = _keyComparer.GetHashCode(key);
                var targetEntry = FindEntry(hashCode, key);
                if (targetEntry == null)
                    throw new KeyNotFoundException($"There is no entry with key \"{key}\"");

                var value = targetEntry.ReadValueVolatile();
                if (value == Entry.Tombstone)
                    throw new KeyNotFoundException($"There is no entry with key \"{key}\"");

                return (TValue) value;
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                var hashCode = _keyComparer.GetHashCode(key);
                var entry = new Entry<TKey, TValue>(hashCode, key, value);
                var addInfo = TryAddinternal(entry);
                if (addInfo.OperationResult == AddResult.AddSuccessful)
                    return;

                var updateInfo = addInfo.TargetEntry.TryUpdateValue(value);
                if (updateInfo.WasUpdateSuccessful == false)
                    throw new InvalidOperationException($"Could not update entry with key {key} because another thread performed this action.");
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(Volatile.Read(ref _currentArray));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>) this).GetEnumerator();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            Clear();
        }

        private ConcurrentArray<TKey, TValue> CreateInitialArray()
        {
            return new ConcurrentArray<TKey, TValue>(_growArrayStrategy.GetInitialSize(), _keyComparer);
        }

        private Entry<TKey, TValue> FindEntry(int hashCode, TKey key)
        {
            return Volatile.Read(ref _currentArray).Find(hashCode, key);
        }

        private static bool GetOrAddOnExistingEntry(Entry<TKey, TValue> targetEntry, Func<TValue> createValue, out TValue value)
        {
            var currentValue = targetEntry.ReadValueVolatile();

            // If the value is no tomb stone, then this means that this entry is not a removed entry,
            // thus the target value was found.
            if (currentValue != Entry.Tombstone)
            {
                value = (TValue) currentValue;
                return false;
            }

            // If the value is a tomb stone, then try to reactivate the entry
            var createdValue = createValue();
            var updateResult = targetEntry.TryUpdateValue(createdValue, Entry.Tombstone);
            if (updateResult.WasUpdateSuccessful)
            {
                value = createdValue;
                return true;
            }

            // If the update didn't work, then this means that the entry has just been reactivated by another thread
            // Just return the read value
            value = (TValue) updateResult.ActualValue;
            return false;
        }

        private ConcurrentArray<TKey, TValue>.AddInfo TryAddinternal(Entry<TKey, TValue> entry)
        {
            // Get the internal array and try to add the entry to it
            var array = Volatile.Read(ref _currentArray);

            TryAdd:
            var addInfo = array.TryAdd(entry);

            // If the entry is already present, then do nothing
            if (addInfo.OperationResult == AddResult.ExistingEntryFound)
                return addInfo;

            // If the entry was added, then increment the count and help copying if necessary
            if (addInfo.OperationResult == AddResult.AddSuccessful)
            {
                Interlocked.Increment(ref _count);
                HelpCopying(array, addInfo);
                return addInfo;
            }

            // Else the internal array is full, we must escalate copying to the new array and then retry the add operation
            var fullArray = array;
            EscalateCopying(array, addInfo);
            // Spin until the new array is available, then try to insert again
            do
            {
                array = Volatile.Read(ref _currentArray);
            } while (fullArray == array);
            goto TryAdd;
        }

        private void HelpCopying(ConcurrentArray<TKey, TValue> array, ConcurrentArray<TKey,TValue>.AddInfo addInfo)
        {
            // Try to get an existing grow-array-process
            var growArrayProcess = array.ReadGrowArrayProcessVolatile();

            // If there is none, then try to create it
            if (growArrayProcess == null)
            {
                var newSize = _growArrayStrategy.GetNextCapacity(array.Count, array.Capacity, addInfo.ReprobingCount);
                // If no new size was proposed by the grow-array-strategy, then we do not need to create a grow-array-process
                if (newSize == null)
                    return;

                // Try to establish a new grow-array-process. This is a race condition because
                // other threads might be doing the same, thus we might get a process object
                // that was created by another thread.
                var processInfo = array.GetOrCreateGrowArrayProcess(newSize.Value, _setNewArray);

                // If we indeed created the process object, then the initial creation of the new target array
                // was already performed (as well as copying up to 100 items). We do not have to perform anything
                // else here.
                if (processInfo.IsProcessFreshlyInitialized)
                    return;

                // If the process object was created on another thread, then help copying
                growArrayProcess = processInfo.TargetProcess;
            }

            // Try to copy the entry to the new array (that was previously added to the old array).
            // This might be necessary if the concurrent copy algorithm is already past this entry.
            // However, this add operation will not result in a AddResult.ArrayFull message because
            // we could insert it in the old array and newArray.Capacity is always greater than oldArray.Capacity.
            growArrayProcess.CopySingleEntry(addInfo.TargetEntry);

            // Try to help copying the rest of the items over to the new array
            growArrayProcess.HelpCopying();
        }

        private void EscalateCopying(ConcurrentArray<TKey, TValue> array, ConcurrentArray<TKey, TValue>.AddInfo addInfo)
        {
            // This method is called when the old array is full
            // It will help copying all remaining items from the old to the new array
            var growArrayProcess = array.ReadGrowArrayProcessVolatile();
            if (growArrayProcess == null)
            {
                // If the grow array process is not created yet (although the old array is already full), then create it now
                var newSize = _growArrayStrategy.GetNextCapacity(array.Count, array.Capacity, addInfo.ReprobingCount);
                if (newSize == null) 
                    throw new InvalidOperationException($"The {nameof(IGrowArrayStrategy)} \"{_growArrayStrategy}\" does not provide a new size although the internal array of the dictionary is full.");

                growArrayProcess = array.GetOrCreateGrowArrayProcess(newSize.Value, _setNewArray).TargetProcess;
            }

            // Copy all remaining element from the old to the new array
            growArrayProcess.CopyToTheBitterEnd();
        }

        private void SetNewArray(ConcurrentArray<TKey, TValue> oldArray, ConcurrentArray<TKey, TValue> newArray)
        {
            Interlocked.CompareExchange(ref _currentArray, newArray, oldArray);
        }

        private class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly ConcurrentArray<TKey, TValue> _array;
            private KeyValuePair<TKey, TValue> _current;
            private int _currentIndex;

            public Enumerator(ConcurrentArray<TKey, TValue> array)
            {
                _array = array;
                _currentIndex = -1;
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (_currentIndex + 1 == _array.Capacity)
                        return false;

                    var currentEntry = _array.ReadVolatileFromIndex(++_currentIndex);
                    if (currentEntry == null)
                        continue;

                    var currentValue = currentEntry.ReadValueVolatile();
                    if (currentValue == Entry.Tombstone)
                        continue;

                    _current = new KeyValuePair<TKey, TValue>(currentEntry.Key, (TValue) currentValue);
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

        public class Options
        {
            public static readonly Options Default = new Options();
            private IGrowArrayStrategy _growArrayStrategy = new LinearDoublingPrimeStrategy();
            private IEqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;
            private IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;

            public IEqualityComparer<TKey> KeyComparer
            {
                get { return _keyComparer; }
                set
                {
                    value.MustNotBeNull();
                    _keyComparer = value;
                }
            }

            public IGrowArrayStrategy GrowArrayStrategy
            {
                get { return _growArrayStrategy; }
                set
                {
                    value.MustNotBeNull();
                    _growArrayStrategy = value;
                }
            }

            public IEqualityComparer<TValue> ValueComparer
            {
                get { return _valueComparer; }
                set
                {
                    value.MustNotBeNull();
                    _valueComparer = value;
                }
            }
        }
    }
}