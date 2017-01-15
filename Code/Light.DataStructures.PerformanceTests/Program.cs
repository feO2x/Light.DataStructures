using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Light.DataStructures.PerformanceTests
{
    public static class Program
    {
        public static void Main(string[] arguments)
        {
            try
            {
                var commandArgumentsMapping = new CommandLineArgumentsParser().ParseArguments(arguments);

                var dictionary = InstantiateDictionary(commandArgumentsMapping);
                var test = InstantiateTest(commandArgumentsMapping);

                var testResults = test.Run(dictionary);

                testResults.Print();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static IDictionary<int, object> InstantiateDictionary(Dictionary<string, string> commandArgumentsMapping)
        {
            string dictionaryTypeName;
            if (commandArgumentsMapping.TryGetValue("testTarget", out dictionaryTypeName) == false)
                throw new ArgumentException("\"testTarget\" is not specified in command line arguments.");

            switch (dictionaryTypeName)
            {
                case "Dictionary":
                    return new Dictionary<int, object>();
                case "ConcurrentDictionary":
                    return new ConcurrentDictionary<int, object>();
                case "LockFreeDictionary":
                    return new LockFreeArrayBasedDictionary<int, object>();
            }

            throw new ArgumentException($"Invalid testTarget name: {dictionaryTypeName}");
        }

        public static IPerformanceTest InstantiateTest(Dictionary<string, string> commandArgumentsMapping)
        {
            string testName;
            if (commandArgumentsMapping.TryGetValue("test", out testName) == false)
                throw new ArgumentException("\"test\" is not specified in command line arguments");

            var targetType = typeof(Program).Assembly
                                            .ExportedTypes
                                            .FirstOrDefault(t => t.Name == testName);

            if (targetType == null)
                throw new ArgumentException($"Test \"{testName}\" does not exist.");

            var createMethod = targetType.GetMethods()
                                         .FirstOrDefault(m => m.IsStatic &&
                                                              m.ReturnType == typeof(IPerformanceTest) &&
                                                              m.GetParameters().Single(p => p.ParameterType == typeof(IDictionary<string, string>)) != null);

            if (createMethod == null)
                return (IPerformanceTest) Activator.CreateInstance(targetType);

            return (IPerformanceTest) createMethod.Invoke(null, new object[] { commandArgumentsMapping });
        }
    }
}