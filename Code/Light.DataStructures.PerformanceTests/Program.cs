using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Light.DataStructures.PerformanceTests
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var test = args[0].InstantiateTest();

            var results = test.Run(args[1].InstantiateDictionary());

            results.Print();
        }

        public static IDictionary<int, object> InstantiateDictionary(this string dictionaryName)
        {
            if (dictionaryName == "Dictionary")
                return new Dictionary<int, object>();
            if (dictionaryName == "ConcurrentDictionary")
                return new ConcurrentDictionary<int, object>();
            if (dictionaryName == "LockFreeDictionary")
                return new LockFreeArrayBasedDictionary<int, object>();

            throw new ArgumentException($"Invalid dictionary name: {dictionaryName}", nameof(dictionaryName));
        }

        public static IPerformanceTest InstantiateTest(this string testName)
        {
            var targetType = typeof(Program).Assembly
                                            .ExportedTypes
                                            .First(t => t.Name == testName);

            return (IPerformanceTest) Activator.CreateInstance(targetType);
        }
    }
}