using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light.DataStructures.PrecompiledDictionaryServices
{
    public sealed class PrecompiledDictionaryFactory
    {
        

        public PrecompiledDictionary<TKey, TValue> Create<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> equalityComparer)
        {
            
        }
    }
}
