using System;
using System.Collections.Generic;
using System.Threading;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public class ConcurrentArray<TKey, TValue>
    {
        private readonly Entry<TKey, TValue>[] _internalArray;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private int _count;

        public ConcurrentArray(int capacity) : this(capacity, EqualityComparer<TKey>.Default) { }

        public ConcurrentArray(int capacity, IEqualityComparer<TKey> keyComparer)
        {
            capacity.MustNotBeLessThan(0, nameof(capacity));
            keyComparer.MustNotBeNull(nameof(keyComparer));

            _internalArray = new Entry<TKey, TValue>[capacity];
            _keyComparer = keyComparer;
        }

        public int Capacity => _internalArray.Length;
        public int Count => Volatile.Read(ref _count);

        public AddInfo TryAdd(Entry<TKey, TValue> entry)
        {
            var startIndex = GetTargetBucketIndex(entry.HashCode);
            var targetIndex = startIndex;

            while (true)
            {
                var previousEntry = Interlocked.CompareExchange(ref _internalArray[targetIndex], entry, null);
                if (previousEntry == null)
                {
                    Interlocked.Increment(ref _count);
                    return new AddInfo(AddResult.AddSuccessful, entry);
                }

                if (entry.HashCode == previousEntry.HashCode && _keyComparer.Equals(entry.Key, previousEntry.Key))
                    return new AddInfo(AddResult.ExistingEntryFound, previousEntry);

                IncrementTargetIndex(ref targetIndex);
                if (targetIndex == startIndex)
                    return new AddInfo(AddResult.ArrayFull, null);
            }
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
            var startIndex = GetTargetBucketIndex(hashCode);
            var targetIndex = startIndex;
            while (true)
            {
                var targetEntry = Volatile.Read(ref _internalArray[targetIndex]);
                if (targetEntry == null)
                    return null;
                if (targetEntry.HashCode == hashCode && _keyComparer.Equals(key, targetEntry.Key))
                    return targetEntry;

                IncrementTargetIndex(ref targetIndex);
                if (startIndex == targetIndex)
                    return null;
            }
        }

        public struct AddInfo
        {
            public readonly AddResult OperationResult;
            public readonly Entry<TKey, TValue> TargetEntry;

            public AddInfo(AddResult operationResult, Entry<TKey, TValue> targetEntry)
            {
                OperationResult = operationResult;
                TargetEntry = targetEntry;
            }
        }
    }
}