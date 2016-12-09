namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public interface IGrowArrayStrategy
    {
        int GetInitialSize();
        int? GetNextCapacity(int currentCount, int currentCapacity, int reprobingCount);
    }
}