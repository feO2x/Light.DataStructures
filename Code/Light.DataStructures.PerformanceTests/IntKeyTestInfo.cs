using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;

namespace Light.DataStructures.PerformanceTests
{
    public static class IntKeyTestInfo
    {
        public const int DefaultNumberOfKeys = 10000000;
        public const string NumberOfKeys = "numberOfKeys";

        public static int[] CreateKeys(int numberOfKeys)
        {
            return Enumerable.Range(1, numberOfKeys)
                             .GroupBy(number => number % 27)
                             .SelectMany(group => group)
                             .ToArray();
        }

        public static int GetNumberOfKeysFromArguments(IDictionary<string, string> arguments)
        {
            arguments.MustNotBeNull();

            var numberOfKeys = DefaultNumberOfKeys;
            string numberOfKeysText;
            if (arguments.TryGetValue(NumberOfKeys, out numberOfKeysText))
                numberOfKeys = Convert.ToInt32(numberOfKeysText);

            return numberOfKeys;
        }
    }
}