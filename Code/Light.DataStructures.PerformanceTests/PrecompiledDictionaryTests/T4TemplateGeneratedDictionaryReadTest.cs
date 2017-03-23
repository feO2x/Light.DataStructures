using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using Light.GuardClauses;
using Newtonsoft.Json;

namespace Light.DataStructures.PerformanceTests.PrecompiledDictionaryTests
{
    public class T4TemplateGeneratedDictionaryReadTest
    {
        private readonly CustomDictionary _customDictionary = new CustomDictionary();
        private Dictionary<int, object> _dictionary;
        private List<int> _keys;

        [Setup]
        public void Setup()
        {
            var json = File.ReadAllText(FileNames.Items10000);
            _keys = JsonConvert.DeserializeObject<List<int>>(json);

            _dictionary = new Dictionary<int, object>();
            foreach (var key in _keys)
            {
                _dictionary.Add(key, new object());
            }
        }

        [Benchmark]
        public Dictionary<int, object> Dictionary()
        {
            foreach (var key in _keys)
            {
                if (_dictionary.TryGetValue(key, out object value))
                    value.MustNotBeNull();
            }

            return _dictionary;
        }

        [Benchmark]
        public CustomDictionary CustomDictionary()
        {
            foreach (var key in _keys)
            {
                if (_customDictionary.TryGetValue(key, out object value))
                    value.MustNotBeNull();
            }

            return _customDictionary;
        }
    }
}