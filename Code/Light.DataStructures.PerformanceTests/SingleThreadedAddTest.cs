using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Light.GuardClauses;

namespace Light.DataStructures.PerformanceTests
{
    public class SingleThreadedAddTest
    {
        private static readonly List<int> Keys10000;
        private static readonly List<int> Keys100000;
        private static readonly List<int> Keys1000000;
        private List<int> _keys;

        static SingleThreadedAddTest()
        {
            Keys10000 = InitializeList(10_000, 42);
            Keys100000 = InitializeList(100_000, 50400302);
            Keys1000000 = InitializeList(1_000_000, 9812903);
        }

        [Params(10_000, 100_000, 1_000_000)]
        public int NumberOfItems { get; set; }


        private static List<int> InitializeList(int numberOfItems, int seed)
        {
            var random = new Random(seed);
            var list = new List<int>(numberOfItems);

            while (list.Count < numberOfItems)
            {
                var number = random.Next();
                if (list.Contains(number))
                    continue;

                list.Add(number);
            }

            return list;
        }

        [Setup]
        public void Setup()
        {
            NumberOfItems.MustNotBe(0);

            if (NumberOfItems == 10_000)
                _keys = Keys10000;
            else if (NumberOfItems == 100_000)
                _keys = Keys100000;
            else
                _keys = Keys1000000;
        }

        [Benchmark]
        public Dictionary<int, object> Dictionary()
        {
            var dictionary = new Dictionary<int, object>();

            foreach (var key in _keys)
            {
                dictionary.Add(key, new object());
            }

            return dictionary;
        }

        [Benchmark]
        public ConcurrentDictionary<int, object> ConcurrentDictionary()
        {
            var concurrentDictionary = new ConcurrentDictionary<int, object>();

            foreach (var key in _keys)
            {
                concurrentDictionary.TryAdd(key, new object());
            }

            return concurrentDictionary;
        }

        [Benchmark]
        public LockFreeArrayBasedDictionary<int, object> LockFreeArrayBasedDictionary()
        {
            var lockFreeDictionary = new LockFreeArrayBasedDictionary<int, object>();

            foreach (var key in _keys)
            {
                lockFreeDictionary.TryAdd(key, new object());
            }

            return lockFreeDictionary;
        }
    }
}