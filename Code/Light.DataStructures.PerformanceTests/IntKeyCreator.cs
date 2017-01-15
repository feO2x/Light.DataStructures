using System.Linq;

namespace Light.DataStructures.PerformanceTests
{
    public static class IntKeyCreator
    {
        public static int[] CreateKeys(int numberOfKeys)
        {
            return Enumerable.Range(1, numberOfKeys)
                             .GroupBy(number => number % 27)
                             .SelectMany(group => group)
                             .ToArray();
        }
    }
}