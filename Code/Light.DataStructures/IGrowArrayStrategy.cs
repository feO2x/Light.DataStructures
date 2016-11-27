namespace Light.DataStructures
{
    public interface IGrowArrayStrategy
    {
        int GetInitialSize();
        int GetNextSize(int currentSize);
    }
}