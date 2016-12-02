namespace Light.DataStructures.LockFreeArrayBasedServices
{
    public interface IGrowArrayProcess
    {
        void StartCopying();
        void HelpCopying();
        void CopyToTheBitterEnd();
    }
}