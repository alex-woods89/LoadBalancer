using LoadBalancer.Interfaces;
using LoadBalancer.Models;
using LoadBalancer.Services;
using Moq;

namespace LoadBalancer.Tests
{
    public class HealthCheckerTests
    {
        [Fact]
        public async Task HealthCheck_OnlyReturnsHealthyBackends()
        {
            var servers = new List<BackendNode>
            {
                new("127.0.0.1", 1234),
                new("127.0.0.1", 4321)
            };
            var mockFactory = new Mock<ITcpClientFactory>();

            mockFactory.Setup(f => f.TryConnectAsync("127.0.0.1", 1234, 100, default)).Returns(Task.FromResult(true));

            mockFactory.Setup(f => f.TryConnectAsync("127.0.0.1", 4321, 100, default)).Returns(Task.FromResult(false));

            var healthChecker = new TcpHealthChecker(mockFactory.Object, 100);
            var result = await healthChecker.GetHealthyNodesAsync(servers, default);

            Assert.Single(result);
            Assert.Equal(1234, result[0].Port);
        }

        [Fact]
        public async Task HealthCheck_OnlyReturnsHealthyBackendsNotInMaintenanceMode()
        {
            var servers = new List<BackendNode>
            {
                new("127.0.0.1", 1234),
                new("127.0.0.1", 4321)
            };

            servers[0].MaintenanceMode = true;

            var mockFactory = new Mock<ITcpClientFactory>();

            mockFactory.Setup(f => f.TryConnectAsync("127.0.0.1", 1234, 100, default)).Returns(Task.FromResult(true));

            mockFactory.Setup(f => f.TryConnectAsync("127.0.0.1", 4321, 100, default)).Returns(Task.FromResult(true));

            var healthChecker = new TcpHealthChecker(mockFactory.Object, 100);
            var result = await healthChecker.GetHealthyNodesAsync(servers, default);

            Assert.Single(result);
            Assert.Equal(4321, result[0].Port);
        }
    }
}
