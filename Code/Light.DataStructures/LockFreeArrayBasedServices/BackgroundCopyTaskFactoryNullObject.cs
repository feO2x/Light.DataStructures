using System;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public sealed class BackgroundCopyTaskFactoryNullObject : IBackgroundCopyTaskFactory
    {
        public void StartBackgroundCopyTask(Action copyFromOldToNewArray)
        {
            
        }
    }
}