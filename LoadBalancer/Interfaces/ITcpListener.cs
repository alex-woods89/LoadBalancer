namespace LoadBalancer.Interfaces
{
    public interface ITcpListener
    {
        void Start();
        void Stop();
        bool Pending();
        Task<ITcpClient> AcceptTcpClientAsync();
    }
}
