using LoadBalancer.Interfaces;
using LoadBalancer.Models;
using LoadBalancer.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text;

namespace LoadBalancer.Tests
{
    public class LoadBalancerRunnerTests
    {
        [Fact]
        public async Task RunAsync_ForwardsRequestToBackend_WhenBackendIsHealthy()
        {
            // Arrange
            var mockHealthChecker = new Mock<IHealthChecker>();
            var mockRoutingStrategy = new Mock<IRoutingStrategy>();
            var mockTcpListener = new Mock<ITcpListener>();
            var mockTcpClientFactory = new Mock<ITcpClientFactory>();
            var mockTcpListenerFactory = new Mock<ITcpListenerFactory>();
            var mockClient = new Mock<ITcpClient>();
            var mockBackend = new Mock<ITcpClient>();

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            var fakeBackendNode = new BackendNode("127.0.0.1", 1234);

            var backendNodes = new List<BackendNode> { fakeBackendNode };

            var configData = new[]
            {
                new KeyValuePair<string, string>("ListenPort", "9000"),
                new KeyValuePair<string, string>("HealthCheckDelay", "1"),
                new KeyValuePair<string, string>("BackendNodes:0:Host", "127.0.0.1"),
                new KeyValuePair<string, string>("BackendNodes:0:Port", "8080")
            };

            var fakeConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Setup health checker returns healthy backend
            mockHealthChecker
                .Setup(h => h.GetHealthyNodesAsync(It.IsAny<List<BackendNode>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(backendNodes);

            // Setup routing strategy selects our backend
            mockRoutingStrategy
                .Setup(r => r.SelectNext(It.IsAny<List<BackendNode>>()))
                .Returns(fakeBackendNode);

            // Setup listener
            mockTcpListener
                .SetupSequence(l => l.Pending())
                .Returns(true)
                .Returns(false); // one connection only

            mockTcpListener
                .Setup(l => l.AcceptTcpClientAsync())
                .ReturnsAsync(mockClient.Object);

            mockTcpListenerFactory
                .Setup(f => f.Create(It.IsAny<int>()))
                .Returns(mockTcpListener.Object);

            // Setup TcpClient factory returns backend mock
            mockTcpClientFactory
                .Setup(f => f.Create())
                .Returns(mockBackend.Object);

            // Mock TCP streams
            // Create in-memory streams
            var clientToBackendStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello from client"));
            var backendToClientStream = new MemoryStream();
            var backendInputStream = new MemoryStream();
            var clientOutputStream = new MemoryStream();

            // Setup mock GetStream
            mockClient.Setup(c => c.GetStream()).Returns(new DuplexStream(clientToBackendStream, clientOutputStream));
            mockBackend.Setup(b => b.GetStream()).Returns(new DuplexStream(backendInputStream, backendToClientStream));

            // Act
            var runner = new LoadBalancerRunner(
                mockHealthChecker.Object,
                mockRoutingStrategy.Object,
                new NullLogger<LoadBalancerRunner>(),
                fakeConfig,
                mockTcpClientFactory.Object,
                mockTcpListenerFactory.Object
            );

            await runner.RunAsync(cts.Token);

            //Assert
            mockBackend.Verify(b => b.ConnectAsync(fakeBackendNode.Host, fakeBackendNode.Port), Times.Once);
        }
    }
}
