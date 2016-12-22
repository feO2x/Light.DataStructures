using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Light.DataStructures.PerformanceTests
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var targetDictionary = args[0].InstantiateTarget();

            var test = new SingleThreadedAddTest();
            var results = test.Run(targetDictionary);

            results.Print();
        }
    }

    public static class StringExtensions
    {
        public static IDictionary<int, object> InstantiateTarget(this string dictionaryName)
        {
            if (dictionaryName == "Dictionary")
                return new Dictionary<int, object>();
            if (dictionaryName == "ConcurrentDictionary")
                return new ConcurrentDictionary<int, object>();
            if (dictionaryName == "LockFreeDictionary")
                return new LockFreeArrayBasedDictionary<int, object>();

            throw new ArgumentException($"Invalid dictionary name: {dictionaryName}", nameof(dictionaryName));
        }
    }
}