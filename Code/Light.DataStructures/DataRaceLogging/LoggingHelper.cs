using System.Diagnostics;

namespace Light.DataStructures.DataRaceLogging
{
    public static class LoggingHelper
    {
        public static int NextId;

        public static IConcurrentLogger Logger;

        [Conditional("CONCURRENT_LOGGING")]
        public static void Log(string message)
        {
            Logger.Log(message);
        }
    }
}