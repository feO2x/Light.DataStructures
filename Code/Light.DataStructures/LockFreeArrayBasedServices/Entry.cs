using System;
using System.Threading;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public abstract class Entry
    {
        public static readonly object Tombstone = new object();
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

        public object Value => Volatile.Read(ref _value);

        public bool TryUpdateValue(TValue newValue)
        {
            return TryUpdateValueInternal(newValue);
        }

        public bool TryMarkAsRemoved()
        {
            return TryUpdateValueInternal(Tombstone);
        }

        private bool TryUpdateValueInternal(object newValue)
        {
            var currentValue = Volatile.Read(ref _value);
            return Interlocked.CompareExchange(ref _value, newValue, currentValue) == currentValue;
        }
    }
}