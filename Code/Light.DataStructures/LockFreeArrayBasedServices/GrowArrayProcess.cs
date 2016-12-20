using System.Threading;
using Light.GuardClauses;
#if CONCURRENT_LOGGING
using Light.DataStructures.DataRaceLogging;
#endif

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public sealed class GrowArrayProcess<TKey, TValue>
#if CONCURRENT_LOGGING
        : ConcurrentLogClient
#endif
    {
        private const int CurrentlyCopying = 0;
        private const int CopyingFinished = 1;
        private const int Aborted = 2;
        public const int DefaultMaximumNumberOfItemsCopiedDuringHelp = 100;
        private readonly ExchangeArray<TKey, TValue> _setNewArray;
        public readonly int MaximumNumberOfItemsCopiedDuringHelp;
        public readonly int NewArraySize;
        public readonly ConcurrentArray<TKey, TValue> OldArray;
        private int _currentIndex = -1;
        private int _state = CurrentlyCopying;
        private ConcurrentArray<TKey, TValue> _newArray;
        private int _numberOfCopiedItems;


        public GrowArrayProcess(ConcurrentArray<TKey, TValue> oldArray,
                                int newArraySize,
                                ExchangeArray<TKey, TValue> setNewArray,
                                int maximumNumberOfItemsCopiedDuringHelp = DefaultMaximumNumberOfItemsCopiedDuringHelp)
        {
            oldArray.MustNotBeNull(nameof(oldArray));
            newArraySize.MustBeGreaterThan(oldArray.Capacity, nameof(newArraySize));
            setNewArray.MustNotBeNull(nameof(setNewArray));
            maximumNumberOfItemsCopiedDuringHelp.MustNotBeLessThan(0, nameof(maximumNumberOfItemsCopiedDuringHelp));

            OldArray = oldArray;
            NewArraySize = newArraySize;
            _setNewArray = setNewArray;
            MaximumNumberOfItemsCopiedDuringHelp = maximumNumberOfItemsCopiedDuringHelp;
        }

        public bool IsCopyingFinished => Volatile.Read(ref _state) == CopyingFinished;
        public bool IsAborted => Volatile.Read(ref _state) == Aborted;
        public ConcurrentArray<TKey, TValue> NewArray => _newArray;

        public void CreateNewArray()
        {
            Volatile.Write(ref _newArray, new ConcurrentArray<TKey, TValue>(NewArraySize, OldArray.KeyComparer));
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
#if CONCURRENT_LOGGING
            if (addInfo.OperationResult == AddResult.AddSuccessful)
                Log($"Copied single entry {entry.Key} to new array {newArray.Id}.");
            else if (addInfo.OperationResult == AddResult.ExistingEntryFound)
                Log($"Tried to copy single entry {entry.Key} to new array {newArray.Id}, but entry is already there.");
            else
                Log($"Tried to copy single entry {entry.Key} to new array {newArray.Id}, but the new array is full.");
#endif
            return addInfo;
        }

        public bool Abort()
        {
            return Interlocked.CompareExchange(ref _state, 2, 0) == 0;
        }

        private bool CopySingleEntry(ConcurrentArray<TKey, TValue> newArray)
        {
            if (ShouldContinueCopying == false)
                return false;

            var currentIndex = Interlocked.Increment(ref _currentIndex);
            if (currentIndex >= OldArray.Capacity)
                return false;

            var entry = OldArray.ReadVolatileFromIndex(currentIndex);
            if (entry != null)
            {
                // ReSharper disable once UnusedVariable
                var addInfo = newArray.TryAdd(entry);

#if CONCURRENT_LOGGING
                if (addInfo.OperationResult == AddResult.AddSuccessful)
                    Log($"Copied entry {entry.Key} to array {newArray.Id}");
                else if (addInfo.OperationResult == AddResult.ExistingEntryFound)
                    Log($"Tried to copy entry {entry.Key} to array {newArray.Id}, but entry was already there.");
                else
                    Log($"Tried to copy entry {entry.Key} to array {newArray.Id}, but the target entry is full. THIS MUST NEVER HAPPEN AND MIGHT LEAD TO LOST ENTRIES.");
#endif
            }

            var numberOfCopiedItems = Interlocked.Increment(ref _numberOfCopiedItems);
            if (numberOfCopiedItems < OldArray.Capacity)
                return true;

            TryFinish(newArray);
            return false;
        }

        private void TryFinish(ConcurrentArray<TKey, TValue> newArray)
        {
            var previousValue = Interlocked.CompareExchange(ref _state, CopyingFinished, CurrentlyCopying);
            if (previousValue == Aborted)
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

        private bool ShouldContinueCopying => Volatile.Read(ref _state) == CurrentlyCopying;
    }
}