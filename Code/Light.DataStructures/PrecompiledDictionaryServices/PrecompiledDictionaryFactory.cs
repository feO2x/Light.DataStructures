using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DataStructures.PrecompiledDictionaryServices
{
    public sealed class PrecompiledDictionaryFactory
    {
        private readonly ILookupFunctionCompiler _compiler;

        public PrecompiledDictionaryFactory(ILookupFunctionCompiler compiler)
        {
            compiler.MustNotBeNull();

            _compiler = compiler;
        }

        public PrecompiledDictionary<TKey, TValue> Create<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            return Create(keyValuePairs, EqualityComparer<TKey>.Default);
        }

        public PrecompiledDictionary<TKey, TValue> Create<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> equalityComparer)
        {
            var lookupFunction = _compiler.CompileDynamicLookupFunction(keyValuePairs, equalityComparer);
            return new PrecompiledDictionary<TKey, TValue>(lookupFunction, keyValuePairs);
        }
    }
}