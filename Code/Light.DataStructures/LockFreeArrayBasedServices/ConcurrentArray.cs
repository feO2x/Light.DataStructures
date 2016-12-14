using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Light.DataStructures.DataRaceLogging;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public class ConcurrentArray<TKey, TValue> : IReadOnlyList<Entry<TKey, TValue>>
    {
        private readonly Entry<TKey, TValue>[] _internalArray;

#if CONCURRENT_LOGGING
        public readonly int Id;
#endif
        public readonly IEqualityComparer<TKey> KeyComparer;
        private int _count;
        private GrowArrayProcess<TKey, TValue> _growArrayProcess;
        private long _version;

        public ConcurrentArray(int capacity) : this(capacity, EqualityComparer<TKey>.Default) { }

        public ConcurrentArray(int capacity, IEqualityComparer<TKey> keyComparer)
        {
            capacity.MustNotBeLessThan(0, nameof(capacity));
            keyComparer.MustNotBeNull(nameof(keyComparer));

            _internalArray = new Entry<TKey, TValue>[capacity];
            KeyComparer = keyComparer;
#if CONCURRENT_LOGGING
            Id = Interlocked.Increment(ref LoggingHelper.NextId);
#endif
        }

        public int Capacity => _internalArray.Length;

        public long Version => Volatile.Read(ref _version);
        public int Count => Volatile.Read(ref _count);

        IEnumerator<Entry<TKey, TValue>> IEnumerable<Entry<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Entry<TKey, TValue> this[int index] => ReadVolatileFromIndex(index);

        public AddInfo TryAdd(Entry<TKey, TValue> entry)
        {
            entry.MustNotBeNull(nameof(entry));

            var startIndex = GetTargetBucketIndex(entry.HashCode);
            var targetIndex = startIndex;

            while (true)
            {
                Interlocked.Increment(ref _version);
                var previousEntry = Interlocked.CompareExchange(ref _internalArray[targetIndex], entry, null);
                if (previousEntry == null)
                {
                    Interlocked.Increment(ref _count);
                    return CreateAddInfo(AddResult.AddSuccessful, entry, startIndex, targetIndex);
                }

                Interlocked.Decrement(ref _version);
                if (entry.HashCode == previousEntry.HashCode && KeyComparer.Equals(entry.Key, previousEntry.Key))
                    return CreateAddInfo(AddResult.ExistingEntryFound, previousEntry, startIndex, targetIndex);

                IncrementTargetIndex(ref targetIndex);
                if (targetIndex == startIndex)
                    return CreateAddInfo(AddResult.ArrayFull, null, startIndex, targetIndex);
            }
        }

        private AddInfo CreateAddInfo(AddResult result, Entry<TKey, TValue> targetEntry, int startIndex, int targetIndex)
        {
            return new AddInfo(result, targetEntry, ArrayMath.CalculateNumberOfSlotsBetween(startIndex, targetIndex, Capacity));
        }

        private int GetTargetBucketIndex(int hashCode)
        {
            return Math.Abs(hashCode) % _internalArray.Length;
        }

        private void IncrementTargetIndex(ref int targetIndex)
        {
            targetIndex = (targetIndex + 1) % _internalArray.Length;
        }

        public Entry<TKey, TValue> Find(int hashCode, TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var startIndex = GetTargetBucketIndex(hashCode);
            var targetIndex = startIndex;
            while (true)
            {
                var targetEntry = Volatile.Read(ref _internalArray[targetIndex]);
                if (targetEntry == null)
                    return null;
                if (targetEntry.HashCode == hashCode && KeyComparer.Equals(key, targetEntry.Key))
                    return targetEntry;

                IncrementTargetIndex(ref targetIndex);
                if (startIndex == targetIndex)
                    return null;
            }
        }

        public Entry<TKey, TValue> ReadVolatileFromIndex(int index)
        {
            return Volatile.Read(ref _internalArray[index]);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_internalArray);
        }

        public GrowArrayProcessInfo CreateOrGetGrowArrayProcess(int newArraySize, ExchangeArray<TKey, TValue> exchangeArray)
        {
            newArraySize.MustBeGreaterThan(Capacity, nameof(newArraySize));

            var newProcess = new GrowArrayProcess<TKey, TValue>(this, newArraySize, exchangeArray);
            var existingProcess = Interlocked.CompareExchange(ref _growArrayProcess, newProcess, null);
            if (existingProcess != null)
                return new GrowArrayProcessInfo(existingProcess, false);

            newProcess.StartCopying();
            return new GrowArrayProcessInfo(newProcess, true);
        }

        public GrowArrayProcess<TKey, TValue> ReadGrowArrayProcessVolatile()
        {
            return Volatile.Read(ref _growArrayProcess);
        }

        public long IncrementVersion()
        {
            return Interlocked.Increment(ref _version);
        }

        public long DecrementVersion()
        {
            return Interlocked.Decrement(ref _version);
        }

        public struct Enumerator : IEnumerator<Entry<TKey, TValue>>
        {
            private readonly Entry<TKey, TValue>[] _internalArray;
            private Entry<TKey, TValue> _current;
            private int _currentIndex;

            public Enumerator(Entry<TKey, TValue>[] internalArray)
            {
                internalArray.MustNotBeNull(nameof(internalArray));

                _internalArray = internalArray;
                _currentIndex = -1;
                _current = null;
            }

            public bool MoveNext()
            {
                while (true)
                {
                    ++_currentIndex;
                    if (_currentIndex == _internalArray.Length)
                    {
                        --_currentIndex;
                        _current = null;
                        return false;
                    }

                    var entry = Volatile.Read(ref _internalArray[_currentIndex]);
                    if (entry == null)
                        continue;

                    var value = entry.ReadValueVolatile();
                    if (value == Entry.Tombstone)
                        continue;

                    _current = entry;
                    return true;
                }
            }

            public void Reset()
            {
                _currentIndex = -1;
                _current = null;
            }

            public Entry<TKey, TValue> Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose() { }
        }

        public struct AddInfo
        {
            public readonly AddResult OperationResult;
            public readonly Entry<TKey, TValue> TargetEntry;
            public readonly int ReprobingCount;

            public AddInfo(AddResult operationResult, Entry<TKey, TValue> targetEntry, int reprobingCount)
            {
                OperationResult = operationResult;
                TargetEntry = targetEntry;
                ReprobingCount = reprobingCount;
            }
        }

        public struct GrowArrayProcessInfo
        {
            public readonly GrowArrayProcess<TKey, TValue> TargetProcess;
            public readonly bool IsProcessFreshlyInitialized;

            public GrowArrayProcessInfo(GrowArrayProcess<TKey, TValue> targetProcess, bool isProcessFreshlyInitialized)
            {
                TargetProcess = targetProcess;
                IsProcessFreshlyInitialized = isProcessFreshlyInitialized;
            }
        }
    }
}