using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DataStructures
{
    public sealed class PrecompiledDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _lookupKey;
        public readonly IReadOnlyList<TKey> Keys;
        public readonly IReadOnlyList<TValue> Values;

        public PrecompiledDictionary(Func<TKey, TValue> lookupKey, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            lookupKey.MustNotBeNull(nameof(lookupKey));
            // ReSharper disable PossibleMultipleEnumeration
            keyValuePairs.MustNotBeNull();

            _lookupKey = lookupKey;
            Keys = keyValuePairs.Select(kvp => kvp.Key).ToArray();
            Values = keyValuePairs.Select(kvp => kvp.Value).ToArray();
            // ReSharper restore PossibleMultipleEnumeration
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int Count => Keys.Count;

        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public TValue this[TKey key] => _lookupKey(key);

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
    }

    public static class PrecompiledDictionary
    {
        public static PrecompiledDictionary<TKey, TValue> CreateFrom<TKey, TValue>(params KeyValuePair<TKey, TValue>[] keyValuePairs)
        {
            keyValuePairs.MustNotBeNull(nameof(keyValuePairs));

            return new PrecompiledDictionary<TKey, TValue>(CreateLookupDelegate(keyValuePairs, EqualityComparer<TKey>.Default), keyValuePairs);
        }

        public static Func<TKey, TValue> CreateLookupDelegate<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> keyComparer)
        {
            keyComparer.MustNotBeNull();

            var keyType = typeof(TKey);
            var keyExpression = Expression.Parameter(keyType, "key");
            var keyComparerExpression = Expression.Constant(keyComparer);
            var hashCodeExpression = Expression.Variable(typeof(int), "hashCode");
            var valueExpression = Expression.Variable(typeof(TValue), "value");
            var getHashCodeMethodInfo = keyComparer.GetType().GetRuntimeMethod(nameof(IEqualityComparer<TKey>.GetHashCode), new[] { keyType });
            var defaultReturnValueExpressions = Expression.Constant(default(TValue));
            var switchCases = keyValuePairs.Select(kvp => Expression.SwitchCase(Expression.Assign(valueExpression, Expression.Constant(kvp.Value)), Expression.Constant(keyComparer.GetHashCode(kvp.Key))))
                                           .ToArray();


            var body = Expression.Block(new[] { hashCodeExpression, valueExpression },
                                        Expression.Assign(hashCodeExpression, Expression.Call(keyComparerExpression, getHashCodeMethodInfo, keyExpression)),
                                        Expression.Switch(hashCodeExpression, Expression.Assign(valueExpression, defaultReturnValueExpressions), switchCases),
                                        valueExpression);

            return Expression.Lambda<Func<TKey, TValue>>(body, keyExpression).Compile();
        }
    }
}