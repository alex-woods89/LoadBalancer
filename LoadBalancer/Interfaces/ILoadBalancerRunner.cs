namespace LoadBalancer.Interfaces
{
    public interface ILoadBalancerRunner 
    {
        Task RunAsync(CancellationToken ct = default);
    }
}
