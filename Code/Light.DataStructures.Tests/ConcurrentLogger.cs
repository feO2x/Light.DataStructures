using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            // ReSharper disable once PossibleInvalidOperationException
            _messages[Interlocked.Increment(ref _currentIndex)] = $"Thread {Thread.CurrentThread.ManagedThreadId:D2} Task {Task.CurrentId.Value:D2}:\t{message}";
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