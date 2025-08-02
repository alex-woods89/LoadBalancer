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
            var backendNodes = new List<BackendNode>
            {
                new("127.0.0.1", 1234),
                new("127.0.0.1", 4321)
            };
            var mockFactory = new Mock<ITcpClientFactory>();

            mockFactory.Setup(f => f.TryConnectAsync("127.0.0.1", 1234, 100, default)).Returns(Task.FromResult(true));

            mockFactory.Setup(f => f.TryConnectAsync("127.0.0.1", 4321, 100, default)).Returns(Task.FromResult(false));

            var healthChecker = new TcpHealthChecker(mockFactory.Object, 100);
            var result = await healthChecker.GetHealthyNodesAsync(backendNodes, default);

            Assert.Single(result);
            Assert.Equal(1234, result[0].Port);
        }

        [Fact]
        public async Task HealthCheck_OnlyReturnsActiveHealthyBackends()
        {
            var backendNodes = new List<BackendNode>
            {
                new("127.0.0.1", 1234),
                new("127.0.0.1", 4321)
            };

            backendNodes[0].IsDeactivated = true;

            var mockFactory = new Mock<ITcpClientFactory>();

            mockFactory.Setup(f => f.TryConnectAsync("127.0.0.1", 1234, 100, default)).Returns(Task.FromResult(true));

            mockFactory.Setup(f => f.TryConnectAsync("127.0.0.1", 4321, 100, default)).Returns(Task.FromResult(true));

            var healthChecker = new TcpHealthChecker(mockFactory.Object, 100);
            var result = await healthChecker.GetHealthyNodesAsync(backendNodes, default);

            Assert.Single(result);
            Assert.Equal(4321, result[0].Port);
        }
    }
}
