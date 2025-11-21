using System.Net;
using System.Net.Sockets;
using Shared.DTOs;

namespace Backend.Services
{
    
        public static class DynamicPortFinder
        {
            public static int GetAvailablePort()
            {
                var listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
                return port;
            }
        }
    
}
