using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Handlers;
using Ampere.Application.SignalR.Queries;
using Ampere.Application.SignalR.Responses;
using Xunit;

namespace Ampere.UnitTests.SignalR;

/// <summary>Tests SignalR telemetry queries.</summary>
public sealed class SignalRQueryTests
{
    [Fact]
    public async Task Snapshot_UsesHouseFilter()
    {
        FakeSignalRService service = new();
        SignalRHandlers handler = new(service);

        Response<TelemetrySnapshotResponse> result =
            await handler.Handle(
                new GetTelemetrySnapshotQuery("house"),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Data?.Items ?? []);
        Assert.Equal("house", service.HouseId);
    }

    private sealed class FakeSignalRService : ISignalRService
    {
        public string? HouseId { get; private set; }

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
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TelemetryResponse>>
            GetTelemetrySnapshotAsync(
                string? houseId,
                CancellationToken cancellationToken)
        {
            HouseId = houseId;
            IReadOnlyList<TelemetryResponse> result =
            [
                new(
                    houseId ?? "house",
                    "node",
                    "endpoint",
                    DateTimeOffset.UtcNow,
                    220,
                    1,
                    220,
                    100,
                    true)
            ];
            return Task.FromResult(result);
        }

        public async IAsyncEnumerable<TelemetryResponse>
            WatchTelemetryAsync(
                string? houseId,
                [System.Runtime.CompilerServices.EnumeratorCancellation]
                CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
