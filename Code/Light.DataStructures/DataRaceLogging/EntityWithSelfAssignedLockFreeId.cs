using System.Threading;

#if CONCURRENT_LOGGING
namespace Light.DataStructures.DataRaceLogging
{
    public abstract class EntityWithSelfAssignedLockFreeId
    {
        private static int _nextId;

        public readonly int Id = Interlocked.Increment(ref _nextId);
    }
}

#endif