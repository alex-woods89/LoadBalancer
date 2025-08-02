namespace LoadBalancer.Models
{
    public class BackendNode
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsDeactivated { get; set; }

        public BackendNode(string host, int port, bool isDeactivated = false)
        {
            Host = host;
            Port = port;
            IsDeactivated = isDeactivated;
        }
    }
}
