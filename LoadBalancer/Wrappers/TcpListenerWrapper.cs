using LoadBalancer.Interfaces;
using System.Net.Sockets;
using System.Net;

namespace LoadBalancer.Wrapper
{
    public class TcpListenerWrapper : ITcpListener
    {
        private readonly TcpListener _listener;

        public TcpListenerWrapper(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start() => _listener.Start();

        public void Stop() => _listener.Stop();

        public bool Pending() => _listener.Pending();

        public async Task<ITcpClient> AcceptTcpClientAsync()
        {
            var tcpClient = await _listener.AcceptTcpClientAsync();
            return new TcpClientWrapper(tcpClient);
        }
    }
}
