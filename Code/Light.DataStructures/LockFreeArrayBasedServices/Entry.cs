using System;
using System.Collections.Generic;
using System.Threading;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public abstract class Entry
    {
        public static readonly object Tombstone = new object();

        public struct UpdateInfo
        {
            public readonly bool WasUpdateSuccessful;
            public readonly object ActualValue;

            public UpdateInfo(bool wasUpdateSuccessful, object actualValue)
            {
                WasUpdateSuccessful = wasUpdateSuccessful;
                ActualValue = actualValue;
            }
        }
    }

    public sealed class Entry<TKey, TValue> : Entry
    {
        public readonly int HashCode;
        public readonly TKey Key;
        private object _value;

        public Entry(int hashCode, TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            HashCode = hashCode;
            Key = key;
            _value = value;
        }

        public UpdateInfo TryUpdateValue(TValue newValue)
        {
            return TryUpdateValueInternal(newValue, Volatile.Read(ref _value));
        }

        public UpdateInfo TryUpdateValue(TValue newValue, object currentValue)
        {
            return TryUpdateValueInternal(newValue, currentValue);
        }

        public UpdateInfo TryMarkAsRemoved()
        {
            return TryUpdateValueInternal(Tombstone, Volatile.Read(ref _value));
        }

        private UpdateInfo TryUpdateValueInternal(object newValue, object currentValue)
        {
            var previousValue = Interlocked.CompareExchange(ref _value, newValue, currentValue);
            var wasUpdateSuccessful = previousValue == currentValue;
            return new UpdateInfo(wasUpdateSuccessful, previousValue);
        }

        public object ReadValueVolatile()
        {
            return Volatile.Read(ref _value);
        }

        public bool IsValueEqualTo(TValue other, IEqualityComparer<TValue> valueComparer)
        {
            var currentValue = Volatile.Read(ref _value);
            if (currentValue == null)
                return other == null;

            if (currentValue == Tombstone)
                return false;

            var downcastedValue = (TValue) currentValue;
            return valueComparer.Equals(downcastedValue, other);
        }
    }
}