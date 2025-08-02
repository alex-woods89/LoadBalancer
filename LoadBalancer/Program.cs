using LoadBalancer;
using LoadBalancer.Factories;
using LoadBalancer.Interfaces;
using LoadBalancer.Models;
using LoadBalancer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true) 
    .Build();

services.AddSingleton<IConfiguration>(configuration);

var backendNodes = configuration.GetSection("BackendNodes").Get<List<BackendNode>>();
services.AddSingleton(backendNodes);

services.AddLogging(config =>
{
    config.AddConsole(); // Could change this to log to a file, or use something like Serilog
});

services.AddTransient<ITcpClientFactory, TcpClientFactory>();
services.AddTransient<IHealthChecker, TcpHealthChecker>();
services.AddTransient<IRoutingStrategy, RoundRobinRouter>();
services.AddTransient<ITcpListenerFactory, TcpListenerFactory>();
services.AddTransient<ILoadBalancerRunner, LoadBalancerRunner>();

using var provider = services.BuildServiceProvider();
var runner = provider.GetRequiredService<ILoadBalancerRunner>();

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

await runner.RunAsync(cts.Token);