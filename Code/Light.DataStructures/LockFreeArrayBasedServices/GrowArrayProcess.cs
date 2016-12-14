using System.Threading;
using System.Threading.Tasks;
using Light.DataStructures.DataRaceLogging;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public sealed class GrowArrayProcess<TKey, TValue>
    {
        private const int MaximumNumberOfItemsCopiedDuringHelp = 100;
        private readonly ExchangeArray<TKey, TValue> _setNewArray;
        public readonly int NewArraySize;
        public readonly ConcurrentArray<TKey, TValue> OldArray;
        private int _currentIndex = -1;
        private int _isCopyingFinished;
        private ConcurrentArray<TKey, TValue> _newArray;
        private int _numberOfCopiedItems;

        public GrowArrayProcess(ConcurrentArray<TKey, TValue> oldArray, int newArraySize, ExchangeArray<TKey, TValue> setNewArray)
        {
            oldArray.MustNotBeNull(nameof(oldArray));
            newArraySize.MustBeGreaterThan(oldArray.Capacity, nameof(newArraySize));
            setNewArray.MustNotBeNull(nameof(setNewArray));

            OldArray = oldArray;
            NewArraySize = newArraySize;
            _setNewArray = setNewArray;
        }

        public bool IsCopyingFinished => Volatile.Read(ref _isCopyingFinished) == 1;
        public ConcurrentArray<TKey, TValue> NewArray => _newArray;

        public void StartCopying()
        {
            Volatile.Write(ref _newArray, new ConcurrentArray<TKey, TValue>(NewArraySize, OldArray.KeyComparer));
            LoggingHelper.Log($"Created and established grow process for array {_newArray.Id}.");

            HelpCopying();
            if (IsCopyingFinished)
                return;

            var task = new Task(CopyToTheBitterEnd);
            task.Start();
            LoggingHelper.Log("Created background task for copying.");
        }

        public void HelpCopying()
        {
            var newArray = SpinGetNewArray();

            for (var i = 0; i < MaximumNumberOfItemsCopiedDuringHelp; ++i)
            {
                if (CopySingleEntry(newArray) == false)
                    return;
            }
        }

        public void CopyToTheBitterEnd()
        {
            var newArray = SpinGetNewArray();
            var shouldContinue = true;
            while (shouldContinue)
            {
                shouldContinue = CopySingleEntry(newArray);
            }
        }

        public ConcurrentArray<TKey, TValue>.AddInfo CopySingleEntry(Entry<TKey, TValue> entry)
        {
            var newArray = SpinGetNewArray();
            var addInfo = newArray.TryAdd(entry);
            if (addInfo.OperationResult == AddResult.AddSuccessful)
                LoggingHelper.Log($"Copied single entry {entry.Key} to new array {newArray.Id}.");
            else if (addInfo.OperationResult == AddResult.ExistingEntryFound)
                LoggingHelper.Log($"Tried to copy single entry {entry.Key} to new array {newArray.Id}, but entry is already there.");
            else
                LoggingHelper.Log($"Tried to copy single entry {entry.Key} to new array {newArray.Id}, but the new array is full.");

            return addInfo;
        }

        public void Abort()
        {
            Interlocked.Exchange(ref _isCopyingFinished, 1);
        }

        private bool CopySingleEntry(ConcurrentArray<TKey, TValue> newArray)
        {
            if (IsCopyingFinished)
                return false;

            var currentIndex = Interlocked.Increment(ref _currentIndex);
            if (currentIndex >= OldArray.Capacity)
                return false;

            var entry = OldArray.ReadVolatileFromIndex(currentIndex);
            if (entry != null)
            {
                var addInfo = newArray.TryAdd(entry);
                if (addInfo.OperationResult == AddResult.AddSuccessful)
                    LoggingHelper.Log($"Copied entry {entry.Key} to array {newArray.Id}");
                else if (addInfo.OperationResult == AddResult.ExistingEntryFound)
                    LoggingHelper.Log($"Tried to copy entry {entry.Key} to array {newArray.Id}, but entry was already there.");
                else
                    LoggingHelper.Log($"Tried to copy entry {entry.Key} to array {newArray.Id}, but the target entry is full. THIS MUST NEVER HAPPEN AND MIGHT LEAD TO LOST ENTRIES.");
            }

            var numberOfCopiedItems = Interlocked.Increment(ref _numberOfCopiedItems);
            if (numberOfCopiedItems < OldArray.Capacity)
                return true;

            TryFinish(newArray);
            return false;
        }

        private void TryFinish(ConcurrentArray<TKey, TValue> newArray)
        {
            if (Interlocked.CompareExchange(ref _isCopyingFinished, 1, 0) == 1)
                return;

            _setNewArray(OldArray, newArray);
        }

        private ConcurrentArray<TKey, TValue> SpinGetNewArray()
        {
            ConcurrentArray<TKey, TValue> newArray;
            do
            {
                newArray = Volatile.Read(ref _newArray);
            } while (newArray == null);
            return newArray;
        }
    }
}