using LoadBalancer.Interfaces;
using LoadBalancer.Models;

namespace LoadBalancer.Services
{
    public class RoundRobinRouter : IRoutingStrategy
    {
        private int _index = 0;
        public BackendNode? SelectNext(List<BackendNode> healthyNodes)
        {
            if (healthyNodes == null || healthyNodes.Count == 0)
            {
                return null;
            }

            int i = Interlocked.Increment(ref _index) - 1;
            return healthyNodes[i % healthyNodes.Count];
        }
    }
}
