using System;

namespace Light.DataStructures.PerformanceTests
{
    public sealed class PerformanceTestResults
    {
        public readonly string Title;
        public readonly string TestTarget;
        public readonly TimeSpan ElapsedTime;
        public PerformanceTestResults(string title, string testTarget, TimeSpan elapsedTime)
        {
            Title = title;
            TestTarget = testTarget;
            ElapsedTime = elapsedTime;
        }

        public void Print()
        {
            Console.WriteLine(Title);
            Console.WriteLine(TestTarget);
            Console.WriteLine(ElapsedTime.TotalMilliseconds.ToString("N"));
        }
    }
}