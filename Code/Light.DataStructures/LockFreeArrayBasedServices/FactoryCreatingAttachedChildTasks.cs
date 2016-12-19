using System;
using System.Threading.Tasks;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public sealed class FactoryCreatingAttachedChildTasks : IBackgroundCopyTaskFactory
    {
        public void StartBackgroundCopyTask(Action copyFromOldToNewArray)
        {
            new Task(copyFromOldToNewArray, TaskCreationOptions.AttachedToParent).Start();
        }
    }
}