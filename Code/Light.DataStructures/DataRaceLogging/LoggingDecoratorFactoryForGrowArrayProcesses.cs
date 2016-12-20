using Light.DataStructures.LockFreeArrayBasedServices;
using Light.GuardClauses;

#if CONCURRENT_LOGGING

namespace Light.DataStructures.DataRaceLogging
{
    public sealed class LoggingDecoratorFactoryForGrowArrayProcesses<TKey, TValue> : IGrowArrayProcessFactory<TKey, TValue>
    {
        private readonly IGrowArrayProcessFactory<TKey, TValue> _decoratedFactory;
        private readonly IConcurrentLogger _logger;

        public LoggingDecoratorFactoryForGrowArrayProcesses(IGrowArrayProcessFactory<TKey, TValue> decoratedFactory, IConcurrentLogger logger)
        {
            decoratedFactory.MustNotBeNull();
            logger.MustNotBeNull();

            _decoratedFactory = decoratedFactory;
            _logger = logger;
        }

        public GrowArrayProcess<TKey, TValue> CreateGrowArrayProcess(ConcurrentArray<TKey, TValue> oldArray, int newArraySize, ExchangeArray<TKey, TValue> setNewArray)
        {
            var growArrayProcess = _decoratedFactory.CreateGrowArrayProcess(oldArray, newArraySize, setNewArray);
            growArrayProcess.Logger = _logger;
            return growArrayProcess;
        }
    }
}

#endif