using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace Light.DataStructures.PerformanceTests
{
    public class MultiThreadedAddTest
    {
        private Dictionary<int, List<int>> _perThreadKeyGroups;

        [Params(FileNames.Items10000, FileNames.Items100000, FileNames.Items500000, FileNames.Items1000000)]
        public string TestFile { get; set; }

        [Setup]
        public void Setup()
        {
            var json = File.ReadAllText(TestFile);
            var keys = JsonConvert.DeserializeObject<List<int>>(json);
            _perThreadKeyGroups = IntegerSequences.DistributeSequencePerCpuCore(keys);
        }

        [Benchmark]
        public Dictionary<int, object> DictionaryInnerLock()
        {
            var dictionary = new Dictionary<int, object>();

            Parallel.ForEach(_perThreadKeyGroups, kvp =>
                                                  {
                                                      foreach (var number in kvp.Value)
                                                      {
                                                          lock (dictionary)
                                                          {
                                                              dictionary.Add(number, new object());
                                                          }
                                                      }
                                                  });
            return dictionary;
        }

        [Benchmark]
        public Dictionary<int, object> DictionaryOuterLock()
        {
            var dictionary = new Dictionary<int, object>();

            Parallel.ForEach(_perThreadKeyGroups, kvp =>
                                                  {
                                                      lock (dictionary)
                                                      {
                                                          foreach (var number in kvp.Value)
                                                          {
                                                              dictionary.Add(number, new object());
                                                          }
                                                      }
                                                  });
            return dictionary;
        }

        [Benchmark]
        public ConcurrentDictionary<int, object> ConcurrentDictionary()
        {
            var dictionary = new ConcurrentDictionary<int, object>();

            Parallel.ForEach(_perThreadKeyGroups, kvp =>
                                                  {
                                                      foreach (var number in kvp.Value)
                                                      {
                                                          dictionary.TryAdd(number, new object());
                                                      }
                                                  });

            return dictionary;
        }

        [Benchmark]
        public LockFreeArrayBasedDictionary<int, object> LockFreeDictionaryArrayBasedDictionary()
        {
            var dictionary = new LockFreeArrayBasedDictionary<int, object>();

            Parallel.ForEach(_perThreadKeyGroups, kvp =>
                                                  {
                                                      foreach (var number in kvp.Value)
                                                      {
                                                          dictionary.TryAdd(number, new object());
                                                      }
                                                  });

            return dictionary;
        }
    }
}