using LoadBalancer.Models;

namespace LoadBalancer.Interfaces
{
    public interface IBackendNodeRepository
    {
        List<BackendNode> GetBackendNodes();
    }
}
