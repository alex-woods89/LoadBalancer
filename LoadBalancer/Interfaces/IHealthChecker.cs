using LoadBalancer.Models;

namespace LoadBalancer.Interfaces
{
    public interface IHealthChecker
    {
        Task<List<BackendNode>> GetHealthyNodesAsync(List<BackendNode> nodes, CancellationToken ct = default);
    }
}
