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
        public readonly ReadOnlyCollection<TKey> Keys;
        public readonly ReadOnlyCollection<TValue> Values;

        public PrecompiledDictionary(LookupDelegate<TKey, TValue> lookup,
                                     IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            lookup.MustNotBeNull();
            // ReSharper disable PossibleMultipleEnumeration
            keyValuePairs.MustNotBeNull();

            _lookup = lookup;
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

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _keyValuePairs.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly => true;

        public void Add(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get { return this[key]; }
            set { throw new NotSupportedException("The precompiled dictionary is read-only."); }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values { get; }

        public TValue this[TKey key]
        {
            get
            {
                _lookup(key, out TValue value);
                return value;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
    }
}