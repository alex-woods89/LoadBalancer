namespace LoadBalancer.Interfaces
{
    public interface ITcpListenerFactory
    {
        ITcpListener Create(int port);
    }
}
