namespace Light.DataStructures.Tests
{
    public static class ConcurrentArrayExtensions
    {
        public static ConcurrentArray<int, object> FillArray(this ConcurrentArray<int, object> targetArray)
        {
            for (var i = 0; i < targetArray.Capacity; i++)
            {
                targetArray.TryAdd(new Entry<int, object>(i.GetHashCode(), i, null));
            }
            return targetArray;
        }
    }
}