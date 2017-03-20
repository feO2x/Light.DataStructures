using System;
using System.Collections.Generic;

namespace Light.DataStructures.PrecompiledDictionaryServices
{
    public interface ILookupFunctionCompiler
    {
        LookupDelegate<TKey, TValue> CompileDynamicLookupFunction<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> keyComparer);
    }
}