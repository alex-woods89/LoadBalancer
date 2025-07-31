using LoadBalancer.Models;

namespace LoadBalancer.Interfaces
{
    public interface IRoutingStrategy
    {
        BackendNode? SelectNext(List<BackendNode> healthyNodes);
    }
}
