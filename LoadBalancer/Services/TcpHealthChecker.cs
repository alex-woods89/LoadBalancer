using LoadBalancer.Interfaces;
using LoadBalancer.Models;

namespace LoadBalancer.Services
{
    public class TcpHealthChecker : IHealthChecker
    {
        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly int _timeoutMs;

        public TcpHealthChecker(ITcpClientFactory tcpClientFactory, int timeoutMs = 1000)
        {
            _tcpClientFactory = tcpClientFactory;
            _timeoutMs = timeoutMs;
        }
        public async Task<List<BackendNode>> GetHealthyNodesAsync(List<BackendNode> nodes, CancellationToken ct = default)
        {
            var healthy = new List<BackendNode>();

            foreach(var node in nodes.Where(n => !n.MaintenanceMode))
            {
                if (await _tcpClientFactory.TryConnectAsync(node.Host, node.Port, _timeoutMs, ct))
                {
                    healthy.Add(node);
                }
            }

            return healthy;
        }
    }
}
