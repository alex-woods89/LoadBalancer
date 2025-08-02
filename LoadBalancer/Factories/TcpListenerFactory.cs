using LoadBalancer.Interfaces;
using LoadBalancer.Wrapper;

namespace LoadBalancer.Factories
{
    public class TcpListenerFactory : ITcpListenerFactory
    {
        public ITcpListener Create(int port) => new TcpListenerWrapper(port);
    }
}
