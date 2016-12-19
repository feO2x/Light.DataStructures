using Light.GuardClauses;

namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public sealed class GrowArrayProcessFactory<TKey, TValue>
    {
        private int _maximumNumberOfItemsCopiedDuringHelp = GrowArrayProcess<TKey, TValue>.DefaultMaximumNumberOfItemsCopiedDuringHelp;

        public int MaximumNumberOfItemsCopiedDuringHelp
        {
            get { return _maximumNumberOfItemsCopiedDuringHelp; }
            set
            {
                value.MustNotBeLessThan(0);
                _maximumNumberOfItemsCopiedDuringHelp = value;
            }
        }

        public GrowArrayProcess<TKey, TValue> CreateGrowArrayProcess(ConcurrentArray<TKey, TValue> oldArray, int newArraySize, ExchangeArray<TKey, TValue> setNewArray)
        {
            return new GrowArrayProcess<TKey, TValue>(oldArray, newArraySize, setNewArray, _maximumNumberOfItemsCopiedDuringHelp);
        }
    }
}