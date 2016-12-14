using System.Threading;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public static class IdHelper
    {
        private static int _nextId;

        public static int GetNextId()
        {
            return Interlocked.Increment(ref _nextId);
        }
    }
}