using System.Threading;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public abstract class EntityWithSelfAssignedLockFreeId
    {
        private static int _nextId;

        public readonly int Id = Interlocked.Increment(ref _nextId);
    }
}