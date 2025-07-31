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

            nodes = FilterMaintenanceMode(nodes);

            foreach (var node in nodes)
            {
                if (await _tcpClientFactory.TryConnectAsync(node.Host, node.Port, _timeoutMs, ct))
                {
                    healthy.Add(node);
                }
            }

            return healthy;
        }

        private List<BackendNode> FilterMaintenanceMode(List<BackendNode> nodes) => nodes.Where(n => !n.MaintenanceMode).ToList();
    }
}
