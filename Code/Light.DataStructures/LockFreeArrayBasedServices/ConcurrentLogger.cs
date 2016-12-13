using System.Diagnostics;
using System.IO;
using System.Threading;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public static class ConcurrentLogger
    {
        private const int MessagesSize = 100000000;
        private static readonly string[] Messages = new string[MessagesSize];
        private static int _currentIndex = -1;

        [Conditional("CONCURRENT_LOGGING")]
        public static void Initialize() { }

        [Conditional("CONCURRENT_LOGGING")]
        public static void Log(string message)
        {
            Messages[Interlocked.Increment(ref _currentIndex)] = message;
        }

        [Conditional("CONCURRENT_LOGGING")]
        public static void WriteLogMessages(TextWriter writer)
        {
            writer.MustNotBeNull();

            for (var i = 0; i < _currentIndex + 1; i++)
            {
                writer.WriteLine(Messages[i]);
            }
        }
    }
}
