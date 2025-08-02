using LoadBalancer.Interfaces;
using LoadBalancer.Models;
using Microsoft.Extensions.Configuration;

namespace LoadBalancer.Repositories
{
    public class BackendNodeRepository : IBackendNodeRepository
    {
        private IConfiguration _config;

        public BackendNodeRepository(IConfiguration config)
        {
            _config = config;
        }
        public List<BackendNode> GetBackendNodes()
        {
            return _config.GetSection("BackendNodes").Get<List<BackendNode>>() ?? [];
        }
    }
}
