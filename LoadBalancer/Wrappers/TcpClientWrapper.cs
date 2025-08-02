using LoadBalancer.Interfaces;
using System.Net.Sockets;
using System.Net;

namespace LoadBalancer.Wrapper
{
    public class TcpClientWrapper : ITcpClient
    {
        private readonly TcpClient _client;

        public TcpClientWrapper()
        {
            _client = new TcpClient();
        }

        internal TcpClientWrapper(TcpClient client)
        {
            _client = client;
        }

        public async Task ConnectAsync(string hostname, int port)
        {
            await _client.ConnectAsync(hostname, port);
        }

        public Stream GetStream() => _client.GetStream();

        public void Close() => _client.Close();

        public EndPoint? RemoteEndPoint => _client.Client?.RemoteEndPoint;

        public void Dispose() => _client.Dispose();
    }

}
