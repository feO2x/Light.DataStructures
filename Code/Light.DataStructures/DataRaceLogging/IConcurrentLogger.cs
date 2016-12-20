
#if CONCURRENT_LOGGING

namespace Light.DataStructures.DataRaceLogging
{
    public interface IConcurrentLogger
    {
        void Log(string message);
    }
}

#endif