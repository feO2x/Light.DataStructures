using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Light.GuardClauses;
using Newtonsoft.Json;

namespace Light.DataStructures.PerformanceTests
{
    public class MultiThreadedReadTest
    {
        private readonly ConcurrentDictionary<int, object> _concurrentDictionary = new ConcurrentDictionary<int, object>();
        private readonly Dictionary<int, object> _dictionary = new Dictionary<int, object>();
        private readonly LockFreeArrayBasedDictionary<int, object> _lockFreeDictionary = new LockFreeArrayBasedDictionary<int, object>();
        private Dictionary<int, List<int>> _perThreadKeys;

        [Params(95)]
        public int PercentageOfElementsPresent { get; set; }

        [Params(FileNames.Items10000)]
        public string TestFile { get; set; }

        [Setup]
        public void Setup()
        {
            var json = File.ReadAllText(TestFile);
            var keys = JsonConvert.DeserializeObject<List<int>>(json);
            FillDictionary(_dictionary, keys);
            FillDictionary(_concurrentDictionary, keys);
            FillDictionary(_lockFreeDictionary, keys);

            _perThreadKeys = IntegerSequences.DistributeSequencePerCpuCore(keys);
        }

        private void FillDictionary(IDictionary<int, object> dictionary, List<int> keys)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                if ((i + 1) % 100 > PercentageOfElementsPresent)
                    continue;

                dictionary.Add(keys[i], new object());
            }
        }

        [Benchmark]
        public Dictionary<int, object> DictionaryWithLockedReadAccess()
        {
            Parallel.ForEach(_perThreadKeys, kvp =>
                                             {
                                                 foreach (var key in kvp.Value)
                                                 {
                                                     lock (_dictionary)
                                                     {
                                                         var result = _dictionary.TryGetValue(key, out object value);
                                                         if (result)
                                                             value.MustNotBeNull();
                                                     }
                                                 }
                                             });

            return _dictionary;
        }

        [Benchmark]
        public ConcurrentDictionary<int, object> ConcurrentDictionary()
        {
            Parallel.ForEach(_perThreadKeys, kvp =>
                                             {
                                                 foreach (var key in kvp.Value)
                                                 {
                                                     var result = _concurrentDictionary.TryGetValue(key, out object value);
                                                     if (result)
                                                         value.MustNotBeNull();
                                                 }
                                             });
            return _concurrentDictionary;
        }

        [Benchmark]
        public LockFreeArrayBasedDictionary<int, object> LockFreeArrayBasedDictionary()
        {
            Parallel.ForEach(_perThreadKeys, kvp =>
                                             {
                                                 foreach (var key in kvp.Value)
                                                 {
                                                     var result = _lockFreeDictionary.TryGetValue(key, out object value);
                                                     if (result)
                                                         value.MustNotBeNull();
                                                 }
                                             });
            return _lockFreeDictionary;
        }
    }
}