namespace LoadBalancer.Interfaces
{
    public interface ITcpClientFactory
    {
        Task<bool> TryConnectAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken = default);

        ITcpClient Create();
    }
}
