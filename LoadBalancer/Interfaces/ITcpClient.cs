using System.Net;

namespace LoadBalancer.Interfaces
{
    public interface ITcpClient : IDisposable
    {
        Task ConnectAsync(string hostname, int port);
        Stream GetStream();
        void Close();
        EndPoint? RemoteEndPoint { get; }
    }
}
