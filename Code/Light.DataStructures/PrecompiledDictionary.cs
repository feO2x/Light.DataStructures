using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Light.DataStructures.PrecompiledDictionaryServices;
using Light.GuardClauses;

namespace Light.DataStructures
{
    public sealed class PrecompiledDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IList<KeyValuePair<TKey, TValue>> _keyValuePairs;
        private readonly LookupDelegate<TKey, TValue> _lookup;
        private readonly IEqualityComparer<TValue> _valueComparer;
        public readonly ReadOnlyCollection<TKey> Keys;
        public readonly ReadOnlyCollection<TValue> Values;

        public PrecompiledDictionary(LookupDelegate<TKey, TValue> lookup,
                                     IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs,
                                     IEqualityComparer<TValue> valueComparer)
        {
            lookup.MustNotBeNull();
            valueComparer.MustNotBeNull();
            // ReSharper disable PossibleMultipleEnumeration
            keyValuePairs.MustNotBeNull();

            _lookup = lookup;
            _valueComparer = valueComparer;
            _keyValuePairs = keyValuePairs.ToArray();
            // ReSharper restore PossibleMultipleEnumeration
            Keys = new ReadOnlyCollection<TKey>(_keyValuePairs.Select(kvp => kvp.Key).ToArray());
            Values = new ReadOnlyCollection<TValue>(_keyValuePairs.Select(kvp => kvp.Value).ToArray());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _keyValuePairs.GetEnumerator();
        }

        public int Count => Keys.Count;

        public bool ContainsKey(TKey key)
        {
#pragma warning disable 168
            return _lookup(key, out TValue value);
#pragma warning restore 168
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _lookup(key, out value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw CreateNotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw CreateNotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _lookup(item.Key, out var value) && _valueComparer.Equals(item.Value, value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _keyValuePairs.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw CreateNotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw CreateNotSupportedException();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw CreateNotSupportedException();
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get { return this[key]; }
            set { throw CreateNotSupportedException(); }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        public TValue this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                     
                if (_lookup(key, out TValue value) == false)
                    throw new KeyNotFoundException($"There is no value for key \"{key}\".");
                return value;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        private static Exception CreateNotSupportedException()
        {
            return new NotSupportedException("The PrecompiledDictionary is read-only.");
        }
    }
}