using Ampere.Application.Common.Abstractions;
using Ampere.Application.SignalR.Responses;

namespace Ampere.UnitTests.Common.Mocks;

public sealed class FakeSignalRService : ISignalRService
{
    public TelemetryResponse? Telemetry { get; private set; }

    public List<TelemetryResponse> StreamItems { get; } = [];

    public Task NotifyDiscoveryAsync(
        DiscoveryResponse response,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task NotifyRelayStateAsync(
        RelayStateResponse response,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task NotifyFirmwareProgressAsync(
        FirmwareProgressResponse response,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task PublishTelemetryAsync(
        TelemetryResponse response,
        CancellationToken cancellationToken)
    {
        Telemetry = response;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TelemetryResponse>>
        GetTelemetrySnapshotAsync(
            string? houseId,
            CancellationToken cancellationToken)
    {
        return Task.FromResult(
            (IReadOnlyList<TelemetryResponse>)[]);
    }

    public async IAsyncEnumerable<TelemetryResponse>
        WatchTelemetryAsync(
            string? houseId,
            [System.Runtime.CompilerServices.EnumeratorCancellation]
                CancellationToken cancellationToken)
    {
        foreach (TelemetryResponse item in StreamItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return item;
        }
    }
}