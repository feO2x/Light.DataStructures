using System.IO;
using System.Threading;
using Light.DataStructures.DataRaceLogging;
using Light.GuardClauses;

namespace Light.DataStructures.Tests
{
    public class ConcurrentLogger : IConcurrentLogger
    {
        private const int MessagesSize = 100000000;
        private readonly string[] _messages = new string[MessagesSize];
        private int _currentIndex = -1;

        public void Log(string message)
        {
            _messages[Interlocked.Increment(ref _currentIndex)] = $"Thread {Thread.CurrentThread.ManagedThreadId}:\t{message}";
        }

        public void WriteLogMessages(TextWriter writer)
        {
            writer.MustNotBeNull();

            for (var i = 0; i < _currentIndex + 1; i++)
            {
                writer.WriteLine(_messages[i]);
            }

            writer.Close();
        }
    }
}