using System;
using System.Threading;

namespace Light.DataStructures
{
    public sealed class Entry<TKey, TValue>
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

        public TValue Value => (TValue) Volatile.Read(ref _value);
    }
}