using Ampere.Application.SignalR.Responses;

namespace Ampere.Application.Common.Abstractions;

/// <summary>Defines server-to-app SignalR operations.</summary>
public interface ISignalRService
{
    /// <summary>Notifies discovery state.</summary>
    Task NotifyDiscoveryAsync(
        DiscoveryResponse response,
        CancellationToken cancellationToken);

    /// <summary>Notifies a relay state change.</summary>
    Task NotifyRelayStateAsync(
        RelayStateResponse response,
        CancellationToken cancellationToken);

    /// <summary>Notifies firmware progress.</summary>
    Task NotifyFirmwareProgressAsync(
        FirmwareProgressResponse response,
        CancellationToken cancellationToken);

    /// <summary>Publishes telemetry to connected apps.</summary>
    Task PublishTelemetryAsync(
        TelemetryResponse response,
        CancellationToken cancellationToken);

    /// <summary>Reads the live telemetry stream.</summary>
    IAsyncEnumerable<TelemetryResponse> WatchTelemetryAsync(
        string? houseId,
        CancellationToken cancellationToken);
}
