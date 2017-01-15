using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DataStructures.PerformanceTests
{
    public sealed class CommandLineArgumentsParser
    {
        public Dictionary<string, string> ParseArguments(string[] arguments)
        {
            var dictionary = new Dictionary<string, string>();
            for (var i = 0; i < arguments.Length; ++i)
            {
                var key = arguments[i];
                key.MustStartWith("-", exception: () => new ArgumentException($"\"{key}\" is not a valid key because it does not start with a hyphen (-)."));

                ++i;
                i.MustBeLessThan(arguments.Length, exception: () => new ArgumentException($"There is no value specified for key \"{key}\"."));

                var value = arguments[i];
                dictionary.Add(key.TrimStart('-'), value);
            }

            return dictionary;
        }
    }
}