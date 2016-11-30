using System;
using System.Collections.Generic;
using System.Threading;
using Light.GuardClauses;

namespace Light.DataStructures
{
    public class ConcurrentArray<TKey, TValue>
    {
        private readonly Entry<TKey, TValue>[] _internalArray;
        private readonly IEqualityComparer<TKey> _keyComparer;

        public ConcurrentArray(int capacity) : this(capacity, EqualityComparer<TKey>.Default) { }

        public ConcurrentArray(int capacity, IEqualityComparer<TKey> keyComparer)
        {
            capacity.MustNotBeLessThan(0, nameof(capacity));
            keyComparer.MustNotBeNull(nameof(keyComparer));

            _internalArray = new Entry<TKey, TValue>[capacity];
            _keyComparer = keyComparer;
        }

        public int Capacity => _internalArray.Length;

        public AddInfo TryAdd(Entry<TKey, TValue> entry)
        {
            var targetIndex = GetTargetBucketIndex(entry.HashCode);

            while (true)
            {
                var previousEntry = Interlocked.CompareExchange(ref _internalArray[targetIndex], entry, null);
                if (previousEntry == null)
                    return new AddInfo(AddResult.AddSuccessful, entry);

                if (entry.HashCode == previousEntry.HashCode && _keyComparer.Equals(entry.Key, previousEntry.Key))
                    return new AddInfo(AddResult.ExistingEntryFound, previousEntry);

                IncrementTargetIndex(ref targetIndex);
            }
        }

        private int GetTargetBucketIndex(int hashCode)
        {
            return Math.Abs(hashCode) % _internalArray.Length;
        }

        private void IncrementTargetIndex(ref int targetIndex)
        {
            targetIndex = targetIndex + 1 % _internalArray.Length;
        }

        public Entry<TKey, TValue> Find(int hashCode, TKey key)
        {
            var targetIndex = GetTargetBucketIndex(hashCode);
            while (true)
            {
                var targetEntry = Volatile.Read(ref _internalArray[targetIndex]);
                if (targetEntry.HashCode == hashCode && _keyComparer.Equals(key, targetEntry.Key))
                    return targetEntry;

                IncrementTargetIndex(ref targetIndex);
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