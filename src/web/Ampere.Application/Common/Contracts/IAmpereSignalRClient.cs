using Ampere.Application.SignalR.Responses;

namespace Ampere.Application.Common.Contracts;

/// <summary>Defines SignalR client notifications.</summary>
public interface IAmpereSignalRClient
{
    /// <summary>Notifies discovery state.</summary>
    Task DiscoveryStateChanged(
        DiscoveryResponse response);

    /// <summary>Notifies a relay state change.</summary>
    Task RelayStateChanged(
        RelayStateResponse response);

    /// <summary>Notifies firmware progress.</summary>
    Task FirmwareUpdateProgress(
        FirmwareProgressResponse response);

    /// <summary>Notifies telemetry updates.</summary>
    Task TelemetryUpdated(
        TelemetryResponse response);
}
