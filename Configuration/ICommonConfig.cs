namespace Configuration
{
    public interface ICommonConfig
    {
        string IpAddress { get; }

        int Port { get; }

        int ServerDelay { get; }

        int ClientDelay { get; }

        int MinValue { get; }

        int MaxValue { get; }
    }
}
