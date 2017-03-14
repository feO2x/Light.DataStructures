using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Light.GuardClauses;
using Newtonsoft.Json;

namespace Light.DataStructures.PerformanceTests
{
    public static class IntegerSequence
    {
        public static List<int> CreateIntegerSequence(int count, int seed)
        {
            count.MustNotBeLessThan(1);

            var random = new Random(seed);
            var list = new List<int>();
            do
            {
                var newNumber = random.Next();
                if (list.Contains(newNumber))
                    continue;

                list.Add(newNumber);
            } while (list.Count < count);

            return list;
        }

        public static void CreateAndSaveIntegerSequence(int count, int seed)
        {
            var integerSequence = CreateIntegerSequence(count, seed);
            var json = JsonConvert.SerializeObject(integerSequence, Formatting.Indented);
            File.WriteAllText($"{count} items seed {seed}.json", json, Encoding.UTF8);
        }
    }
}