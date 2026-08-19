using System.Net;
using System.Net.Sockets;

namespace Ampere.UnitTests.Common.Fixtures;

/// <summary>Provides available local TCP ports.</summary>
public static class MqttTestPort
{
    /// <summary>Gets an available loopback TCP port.</summary>
    /// <returns>The available port number.</returns>
    public static int Get()
    {
        TcpListener listener = new(
            IPAddress.Loopback,
            0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint)
            .Port;
        listener.Stop();
        return port;
    }
}
