using LoadBalancer.Interfaces;
using LoadBalancer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace LoadBalancer
{
    //TODO this class isnt super testable at the moment, need to find a way to inject and mock the TcpListener and TcpClient
    public class LoadBalancerRunner
    {
        private readonly IHealthChecker _healthChecker;
        private readonly IRoutingStrategy _routingStrategy;
        private readonly ILogger<LoadBalancerRunner> _logger;
        private  List<BackendNode> _allBackends;
        private int _healthCheckLoopDelay;
        private readonly IConfiguration _config;
        private volatile List<BackendNode> _healthyBackends = [];
        private int _listenPort { get; set; } 

        public LoadBalancerRunner(IHealthChecker healthChecker, IRoutingStrategy routingStrategy, ILogger<LoadBalancerRunner> logger, IConfiguration config)
        {
            _healthChecker = healthChecker;
            _routingStrategy = routingStrategy;
            _logger = logger;
            _config = config;

            _listenPort = _config.GetValue<int>("ListenPort");
            _allBackends = _config.GetSection("BackendNodes").Get<List<BackendNode>>() ?? new List<BackendNode>();
            _healthCheckLoopDelay = _config.GetValue<int>("HealthCheckDelay");
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Starting Load Balancer...");

            _ = Task.Run(() => HealthCheckLoop(ct), ct);

            var listener = new TcpListener(System.Net.IPAddress.Any, _listenPort);
            listener.Start();
            _logger.LogInformation("Load Balancer started on port {portNumer}.", _listenPort);

            try 
            {
                while (!ct.IsCancellationRequested)
                {
                    if(listener.Pending())
                    {
                        var client = await listener.AcceptTcpClientAsync();
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

                    _logger.LogInformation($"Healthy backends updated: {_healthyBackends.Count} nodes available.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during health check.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_healthCheckLoopDelay), ct);
            }
        }
        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            var currentBackends = _healthyBackends;
            BackendNode? backend = _routingStrategy.SelectNext(currentBackends);

            if (backend == null)
            {
                _logger.LogInformation("No healthy backend available.");
                client.Close();
                return;
            }

            TcpClient backendClient = new TcpClient();
            try
            {
                await backendClient.ConnectAsync(backend.Host, backend.Port);
                _logger.LogInformation($"Forwarding request to backend {backend.Host}:{backend.Port}");

                using var clientStream = client.GetStream();
                using var backendStream = backendClient.GetStream();

                await clientStream.CopyToAsync(backendStream, ct);
                await backendStream.CopyToAsync(clientStream, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling client request for backend {backend.Host}:{backend.Port}");
            }
            finally
            {
                client.Close();
                backendClient.Close();
            }
        }
    }
}
