using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Light.DataStructures.PrecompiledDictionaryServices
{
    public sealed class DefaultLookupFunctionCompiler : ILookupFunctionCompiler
    {
        public LookupDelegate<TKey, TValue> CompileDynamicLookupFunction<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> keyComparer)
        {
            var keyType = typeof(TKey);
            var keyExpression = Expression.Parameter(keyType, "key");
            var valueExpression = Expression.Parameter(typeof(TValue).MakeByRefType(), "value");
            var keyComparerExpression = Expression.Constant(keyComparer);
            var getHashCodeMethodInfo = keyComparer.GetType().GetRuntimeMethod(nameof(IEqualityComparer<TKey>.GetHashCode), new[] { keyType });
            var hashCodeExpression = Expression.Variable(typeof(int), "hashCode");
            var resultExpression = Expression.Variable(typeof(bool), "result");
            var trueExpression = Expression.Constant(true);

            var switchCases = keyValuePairs.Select(kvp => Expression.SwitchCase(Expression.Block(Expression.Assign(valueExpression, Expression.Constant(kvp.Value)),
                                                                                                 Expression.Assign(resultExpression, trueExpression)),
                                                                                Expression.Constant(keyComparer.GetHashCode(kvp.Key))))
                                           .ToArray();


            var body = Expression.Block(new[] { hashCodeExpression, resultExpression },
                                        Expression.Assign(hashCodeExpression, Expression.Call(keyComparerExpression, getHashCodeMethodInfo, keyExpression)),
                                        Expression.Assign(resultExpression, Expression.Constant(false)),
                                        Expression.Switch(typeof(void), hashCodeExpression, Expression.Assign(valueExpression, Expression.Constant(default(TValue), typeof(TValue))), null, switchCases),
                                        resultExpression);

            return Expression.Lambda<LookupDelegate<TKey, TValue>>(body, keyExpression, valueExpression).Compile();
        }
    }
}