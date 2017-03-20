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
        private readonly Func<TKey, bool> _containsKey;
        private readonly Func<TKey, TValue> _lookupKey;
        public readonly IReadOnlyList<TKey> Keys;
        public readonly IReadOnlyList<TValue> Values;

        public PrecompiledDictionary(Func<TKey, TValue> lookupKey,
                                     Func<TKey, bool> containsKey,
                                     IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            lookupKey.MustNotBeNull(nameof(lookupKey));
            containsKey.MustNotBeNull(nameof(containsKey));
            // ReSharper disable PossibleMultipleEnumeration
            keyValuePairs.MustNotBeNull();

            _lookupKey = lookupKey;
            _containsKey = containsKey;
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
            return _containsKey(key);
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
            return CreateFrom(keyValuePairs, EqualityComparer<TKey>.Default);
        }

        public static PrecompiledDictionary<TKey, TValue> CreateFrom<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> keyComparer)
        {
            // ReSharper disable PossibleMultipleEnumeration
            keyValuePairs.MustNotBeNull();
            keyComparer.MustNotBeNull();

            return new PrecompiledDictionary<TKey, TValue>(CreateLookupDelegate(keyValuePairs, EqualityComparer<TKey>.Default),
                                                           CreateContainsKeyDelegate(keyValuePairs, EqualityComparer<TKey>.Default),
                                                           keyValuePairs);
            // ReSharper restore PossibleMultipleEnumeration
        }

        private static Func<TKey, TValue> CreateLookupDelegate<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> keyComparer)
        {
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

        private static Func<TKey, bool> CreateContainsKeyDelegate<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, EqualityComparer<TKey> keyComparer)
        {
            var keyType = typeof(TKey);
            var keyExpression = Expression.Parameter(keyType, "key");
            var keyComparerExpression = Expression.Constant(keyComparer);
            var hashCodeExpression = Expression.Variable(typeof(int), "hashCode");
            var returnValueExpression = Expression.Variable(typeof(bool), "returnValue");
            var getHashCodeMethodInfo = keyComparer.GetType().GetRuntimeMethod(nameof(IEqualityComparer<TKey>.GetHashCode), new[] { keyType });
            var trueExpression = Expression.Constant(true);
            var switchCases = keyValuePairs.Select(kvp => Expression.SwitchCase(Expression.Assign(returnValueExpression, trueExpression), Expression.Constant(keyComparer.GetHashCode(kvp.Key))))
                                           .ToArray();

            var body = Expression.Block(new[] { hashCodeExpression, returnValueExpression },
                                        Expression.Assign(hashCodeExpression, Expression.Call(keyComparerExpression, getHashCodeMethodInfo, keyExpression)),
                                        Expression.Switch(hashCodeExpression, Expression.Assign(returnValueExpression, Expression.Constant(false)), switchCases),
                                        returnValueExpression);

            return Expression.Lambda<Func<TKey, bool>>(body, keyExpression).Compile();
        }
    }
}