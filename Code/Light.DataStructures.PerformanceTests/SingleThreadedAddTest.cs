using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace Light.DataStructures.PerformanceTests
{
    public class SingleThreadedAddTest
    {
        private List<int> _keys;

        [Params(FileNames.Items10000, FileNames.Items100000, FileNames.Items500000, FileNames.Items1000000)]
        public string TestFile { get; set; }

        [Setup]
        public void Setup()
        {
            var json = File.ReadAllText(TestFile);
            _keys = JsonConvert.DeserializeObject<List<int>>(json);
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