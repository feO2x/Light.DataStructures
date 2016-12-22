using System.Collections.Generic;

namespace Light.DataStructures.PerformanceTests
{
    public interface IPerformanceTest
    {
        PerformanceTestResults Run(IDictionary<int, object> dictionary);
    }
}