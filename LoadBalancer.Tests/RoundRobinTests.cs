using LoadBalancer.Models;
using LoadBalancer.Services;


namespace LoadBalancer.Tests
{
    public class RoundRobinTests
    {
        [Fact]
        public void Router_ShouldCycleThroughAvailableBackends() 
        {
            var router = new RoundRobinRouter();
            var servers = new List<BackendNode>
            {
                new("127.0.0.1", 1234),
                new("127.0.0.1", 4321),
                new("127.0.0.1", 5678)
            };
            
            var selections = new List<BackendNode>();

            for(int i = 0; i < 5; i++)
            {
                var selectedNode = router.SelectNext(servers);
                selections.Add(selectedNode);
            }

            Assert.Equal(servers[0], selections[0]);
            Assert.Equal(servers[1], selections[1]);
            Assert.Equal(servers[2], selections[2]);
            Assert.Equal(servers[0], selections[3]);
            Assert.Equal(servers[1], selections[4]);
        }
    }
}
