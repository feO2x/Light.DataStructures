using System;

namespace Light.DataStructures
{
    public sealed class Entry<TKey, TValue>
    {
        public readonly int HashCode;
        public readonly TKey Key;
        public readonly TValue Value;

        public Entry(int hashCode, TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            HashCode = hashCode;
            Key = key;
            Value = value;
        }
    }
}