using System.Threading.Channels;
using Ampere.Application.Common.Abstractions;
using Ampere.Application.SignalR.Responses;
using Microsoft.AspNetCore.SignalR;

namespace Ampere.Infrastructure.Common.Hubs;

/// <summary>Publishes Ampere events through SignalR.</summary>
/// <param name="hubContext">The SignalR hub context.</param>
public sealed class SignalRService(
    IHubContext<Hub> hubContext) : ISignalRService
{
    private readonly Channel<TelemetryResponse> _telemetry =
        Channel.CreateUnbounded<TelemetryResponse>();

    /// <inheritdoc />
    public Task NotifyDiscoveryAsync(
        DiscoveryResponse response,
        CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync(
            "DiscoveryStateChanged",
            response,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task NotifyRelayStateAsync(
        RelayStateResponse response,
        CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync(
            "RelayStateChanged",
            response,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task NotifyFirmwareProgressAsync(
        FirmwareProgressResponse response,
        CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync(
            "FirmwareUpdateProgress",
            response,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishTelemetryAsync(
        TelemetryResponse response,
        CancellationToken cancellationToken)
    {
        await _telemetry.Writer.WriteAsync(
            response,
            cancellationToken);

        await hubContext.Clients.All.SendAsync(
            "TelemetryUpdated",
            response,
            cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TelemetryResponse>
        WatchTelemetryAsync(
            string? houseId,
            [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken)
    {
        await foreach (TelemetryResponse response in
            _telemetry.Reader.ReadAllAsync(cancellationToken))
        {
            if (houseId is null ||
                string.Equals(
                    response.HouseId,
                    houseId,
                    StringComparison.OrdinalIgnoreCase))
            {
                yield return response;
            }
        }
    }
}
