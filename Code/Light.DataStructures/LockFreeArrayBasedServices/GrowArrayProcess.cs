using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public sealed class GrowArrayProcess<TKey, TValue>
    {
        private const int MaximumNumberOfItemsCopiedDuringHelp = 100;
        private readonly ExchangeArray<TKey, TValue> _setNewArray;
        public readonly int NewArraySize;
        public readonly ConcurrentArray<TKey, TValue> OldArray;
        private int _copyingFinished;
        private int _currentIndex = -1;
        private ConcurrentArray<TKey, TValue> _newArray;

        public GrowArrayProcess(ConcurrentArray<TKey, TValue> oldArray, int newArraySize, ExchangeArray<TKey, TValue> setNewArray)
        {
            oldArray.MustNotBeNull(nameof(oldArray));
            newArraySize.MustBeGreaterThan(oldArray.Capacity, nameof(newArraySize));
            setNewArray.MustNotBeNull(nameof(setNewArray));

            OldArray = oldArray;
            NewArraySize = newArraySize;
            _setNewArray = setNewArray;
        }

        public void StartCopying()
        {
            Volatile.Write(ref _newArray, new ConcurrentArray<TKey, TValue>(NewArraySize, OldArray.KeyComparer));

            HelpCopying();
            if (IsCopyingFinished)
                return;

            var task = new Task(CopyToTheBitterEnd);
            task.Start();
        }

        public void HelpCopying()
        {
            var newArray = SpinGetNewArray();

            for (var i = 0; i < MaximumNumberOfItemsCopiedDuringHelp; ++i)
            {
                var shouldContinue = CopySingleEntry(newArray);
                if (shouldContinue == false)
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

        public ConcurrentArray<TKey,TValue>.AddInfo CopySingleEntry(Entry<TKey, TValue> entry)
        {
            var newArray = SpinGetNewArray();
            return newArray.TryAdd(entry);
        }

        public void Abort()
        {
            Interlocked.Exchange(ref _copyingFinished, 1);
        }

        private bool CopySingleEntry(ConcurrentArray<TKey, TValue> newArray)
        {
            if (IsCopyingFinished)
                return false;

            var currentIndex = Interlocked.Increment(ref _currentIndex);
            if (currentIndex >= OldArray.Capacity)
            {
                TryFinish(newArray);
                return false;
            }

            var entry = OldArray.ReadVolatileFromIndex(currentIndex);
            if (entry == null)
                return true;

            newArray.TryAdd(entry);
            return true;
        }

        private void TryFinish(ConcurrentArray<TKey, TValue> newArray)
        {
            if (Interlocked.CompareExchange(ref _copyingFinished, 1, 0) != 0)
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

        public bool IsCopyingFinished => Volatile.Read(ref _copyingFinished) == 1;
        public ConcurrentArray<TKey, TValue> NewArray => _newArray;
    }
}