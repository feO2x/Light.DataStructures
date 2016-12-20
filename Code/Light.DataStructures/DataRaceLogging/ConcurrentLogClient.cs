using Light.GuardClauses;

#if CONCURRENT_LOGGING

namespace Light.DataStructures.DataRaceLogging
{
    public abstract class ConcurrentLogClient
    {
        private IConcurrentLogger _logger;

        public IConcurrentLogger Logger
        {
            get { return _logger; }
            set
            {
                value.MustNotBeNull();
                _logger = value;
            }
        }

        protected void Log(string message)
        {
            _logger?.Log(message);
        }
    }
}

#endif