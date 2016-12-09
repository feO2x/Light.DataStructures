namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public static class ArrayMath
    {
        public static int CalculateNumberOfSlotsBetween(int startIndex, int endIndex, int arrayLength)
        {
            if (endIndex >= startIndex)
                return endIndex - startIndex;

            return arrayLength - startIndex + endIndex;
        }
    }
}