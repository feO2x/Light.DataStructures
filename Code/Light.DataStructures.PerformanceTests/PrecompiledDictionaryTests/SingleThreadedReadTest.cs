using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using Light.DataStructures.PrecompiledDictionaryServices;
using Light.GuardClauses;
using Newtonsoft.Json;

namespace Light.DataStructures.PerformanceTests.PrecompiledDictionaryTests
{
    public class SingleThreadedReadTest
    {
        private ConcurrentDictionary<int, object> _concurrentDictionary;
        private Dictionary<int, object> _dictionary;
        private List<int> _keys;
        private PrecompiledDictionary<int, object> _precompiledDictionary;

        [Params(FileNames.Items10000, FileNames.Items100000, FileNames.Items500000, FileNames.Items1000000)]
        public string TestFile { get; set; }

        [Params(100, 95, 75, 50, 25)]
        public int PercentageOfElementsPresent { get; set; }

        [Setup]
        public void Setup()
        {
            var json = File.ReadAllText(TestFile);
            _keys = JsonConvert.DeserializeObject<List<int>>(json);

            _dictionary = CreateDictionary();
            _concurrentDictionary = new ConcurrentDictionary<int, object>(_dictionary);
            _precompiledDictionary = new PrecompiledDictionaryFactory(new DefaultLookupFunctionCompiler()).Create(_dictionary);
        }

        private Dictionary<int, object> CreateDictionary()
        {
            var dictionary = new Dictionary<int, object>();

            for (var i = 0; i < _keys.Count; i++)
            {
                if ((i + 1) % 100 > PercentageOfElementsPresent)
                    continue;

                dictionary.Add(_keys[i], new object());
            }

            return dictionary;
        }

        [Benchmark]
        public Dictionary<int, object> Dictionary()
        {
            foreach (var key in _keys)
            {
                if (_dictionary.TryGetValue(key, out var value))
                    value.MustNotBeNull();
            }
            return _dictionary;
        }

        [Benchmark]
        public ConcurrentDictionary<int, object> ConcurrentDictionary()
        {
            foreach (var key in _keys)
            {
                if (_concurrentDictionary.TryGetValue(key, out var value))
                    value.MustNotBeNull();
            }

            return _concurrentDictionary;
        }

        [Benchmark]
        public PrecompiledDictionary<int, object> PrecompiledDictionary()
        {
            foreach (var key in _keys)
            {
                if (_precompiledDictionary.TryGetValue(key, out var value))
                    value.MustNotBeNull();
            }

            return _precompiledDictionary;
        }
    }
}