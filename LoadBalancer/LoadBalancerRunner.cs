using LoadBalancer.Factories;
using LoadBalancer.Interfaces;
using LoadBalancer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace LoadBalancer
{
    public class LoadBalancerRunner : ILoadBalancerRunner
    {
        private readonly IHealthChecker _healthChecker;
        private readonly IRoutingStrategy _routingStrategy;
        private readonly ILogger<LoadBalancerRunner> _logger;
        private readonly IConfiguration _config;
        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly ITcpListenerFactory _tcpListenerFactory;

        private int _healthCheckLoopDelay;

        private readonly List<BackendNode> _allBackends;
        private volatile List<BackendNode> _healthyBackends = [];
        private int _listenPort { get; set; }

        public LoadBalancerRunner(IHealthChecker healthChecker, IRoutingStrategy routingStrategy, ILogger<LoadBalancerRunner> logger, IConfiguration config, ITcpClientFactory tcpClientFactory, ITcpListenerFactory tcpListenerFactory)
        {
            _healthChecker = healthChecker;
            _routingStrategy = routingStrategy;
            _logger = logger;
            _config = config;
            _tcpClientFactory = tcpClientFactory;
            _tcpListenerFactory = tcpListenerFactory;

            _listenPort = _config.GetValue<int>("ListenPort");
            _allBackends = _config.GetSection("BackendNodes").Get<List<BackendNode>>() ?? new();
            _healthCheckLoopDelay = _config.GetValue<int>("HealthCheckDelay");
        }


        public async Task RunAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Starting Load Balancer...");

            _ = Task.Run(() => HealthCheckLoop(ct), ct);

            var listener = _tcpListenerFactory.Create(_listenPort);
            listener.Start();
            _logger.LogInformation("Load Balancer started on port {portNumer}.", _listenPort);

            try 
            {
                while (!ct.IsCancellationRequested)
                {
                    if(listener.Pending())
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        _logger.LogInformation("Accepted connection from {clientEndpoint}.", client.RemoteEndPoint);

                        _ = Task.Run(() => HandleClientAsync(client, ct), ct);
                    }
                    else
                    {
                        await Task.Delay(100, ct); // Avoid constantly waiting
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Load Balancer main loop.");
            }
            finally
            {
                listener.Stop();
            }

            _logger.LogInformation("Load Balancer stopped.");
        }

        private async Task HealthCheckLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Performing health check on backend nodes...");
                    var healthyNodes = await _healthChecker.GetHealthyNodesAsync(_allBackends, ct);

                    _healthyBackends = healthyNodes.ToList();

                    _logger.LogInformation("Healthy backends updated: {count} nodes available.", _healthyBackends.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during health check.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_healthCheckLoopDelay), ct);
            }
        }
        private async Task HandleClientAsync(ITcpClient client, CancellationToken ct)
        {
            var currentBackends = _healthyBackends;
            BackendNode? backend = _routingStrategy.SelectNext(currentBackends);

            if (backend == null)
            {
                _logger.LogInformation("No healthy backend available.");
                client.Close();
                return;
            }

            var backendClient = _tcpClientFactory.Create();
            try
            {
                await backendClient.ConnectAsync(backend.Host, backend.Port);
                _logger.LogInformation("Forwarding request to backend {host}:{port}", backend.Host, backend.Port);

                using var clientStream = client.GetStream();
                using var backendStream = backendClient.GetStream();

                await clientStream.CopyToAsync(backendStream, ct);
                await backendStream.CopyToAsync(clientStream, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling client request for backend  {host}:{port}", backend.Host, backend.Port);
            }
            finally
            {
                client.Close();
                backendClient.Close();
            }
        }
    }
}
