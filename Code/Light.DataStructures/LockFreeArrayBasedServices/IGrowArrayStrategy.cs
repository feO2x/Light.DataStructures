namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public interface IGrowArrayStrategy
    {
        int GetInitialSize();
        int GetNextSize(int currentSize);
    }
}