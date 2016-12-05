namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public interface IGrowArrayProcess<TKey, TValue>
    {
        void StartCopying();
        void HelpCopying();
        void CopyToTheBitterEnd();
        void CopySingleEntry(Entry<TKey, TValue> entry);
    }
}