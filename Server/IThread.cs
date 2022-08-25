namespace Server
{
    public interface IThread
    {
        bool IsRunning();

        void ThreadStart();

        void ThreadStop();
    }
}
