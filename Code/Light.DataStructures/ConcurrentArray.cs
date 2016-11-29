using System;
using System.Collections.Generic;
using System.Threading;

namespace Light.DataStructures
{
    public class ConcurrentArray<TKey, TValue>
    {
        private readonly IEqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;
        private readonly Entry<TKey, TValue>[] _internalArray;

        public ConcurrentArray(int capacity)
        {
            _internalArray = new Entry<TKey, TValue>[capacity];
        }

        public int Capacity => _internalArray.Length;

        public AddInfo TryAdd(Entry<TKey, TValue> entry)
        {
            var targetIndex = GetTargetBucketIndex(entry.HashCode);

            while (true)
            {
                var previousEntry = Interlocked.CompareExchange(ref _internalArray[targetIndex], entry, null);
                if (previousEntry == null)
                    return new AddInfo(AddResult.Successful, entry);

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