using System;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public interface IBackgroundCopyTaskFactory
    {
        void StartBackgroundCopyTask(Action copyFromOldToNewArray);
    }
}