using System;
using System.Threading.Tasks;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public sealed class FactoryCreatingIndependentTasks : IBackgroundCopyTaskFactory
    {
        public void StartBackgroundCopyTask(Action copyFromOldToNewArray)
        {
            new Task(copyFromOldToNewArray).Start();
        }
    }
}