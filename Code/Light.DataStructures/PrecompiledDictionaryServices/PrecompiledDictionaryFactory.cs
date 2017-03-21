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

        public PrecompiledDictionary<TKey, TValue> Create<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs,
                                                                        IEqualityComparer<TKey> keyComparer = null,
                                                                        IEqualityComparer<TValue> valueComparer = null)
        {
            keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            // ReSharper disable PossibleMultipleEnumeration
            var lookupFunction = _compiler.CompileDynamicLookupFunction(keyValuePairs, keyComparer);
            return new PrecompiledDictionary<TKey, TValue>(lookupFunction, keyValuePairs, valueComparer);
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}