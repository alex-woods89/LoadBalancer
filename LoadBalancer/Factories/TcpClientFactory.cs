using LoadBalancer.Interfaces;
using System.Net.Sockets;

namespace LoadBalancer.Factories
{
    public class TcpClientFactory : ITcpClientFactory
    {
        public async Task<bool> TryConnectAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken = default)
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var delayTask = Task.Delay(timeoutMs, cancellationToken);

            var completed = await Task.WhenAny(connectTask, delayTask);

            return completed == connectTask && client.Connected;
        }
    }
}
