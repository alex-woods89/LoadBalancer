namespace LoadBalancer.Models
{
    public class BackendNode
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool MaintenanceMode { get; set; }

        public BackendNode(string host, int port, bool maintenanceMode = false)
        {
            Host = host;
            Port = port;
            MaintenanceMode = maintenanceMode;
        }
    }
}
