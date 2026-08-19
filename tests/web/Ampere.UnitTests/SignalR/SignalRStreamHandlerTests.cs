using Ampere.Application.Common.Abstractions;
using Ampere.Application.SignalR.Handlers;
using Ampere.Application.SignalR.Queries;
using Ampere.Application.SignalR.Responses;
using Xunit;

namespace Ampere.UnitTests.SignalR;

/// <summary>Tests the SignalR telemetry stream handler.</summary>
public sealed class SignalRStreamHandlerTests
{
    [Fact]
    public async Task WatchTelemetry_DelegatesHouseFilter()
    {
        FakeSignalRService service = new();
        SignalRStreamHandlers handler = new(service);

        List<TelemetryResponse> result = [];
        await foreach (TelemetryResponse item in handler.Handle(
            new WatchTelemetryQuery("house"),
            CancellationToken.None))
        {
            result.Add(item);
        }

        Assert.Single(result);
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

        public async IAsyncEnumerable<TelemetryResponse>
            WatchTelemetryAsync(
                string? houseId,
                [System.Runtime.CompilerServices.EnumeratorCancellation]
                CancellationToken cancellationToken)
        {
            HouseId = houseId;
            yield return new TelemetryResponse(
                houseId ?? "house",
                "node",
                "endpoint",
                DateTimeOffset.UtcNow,
                220,
                1,
                220,
                100,
                true);
            await Task.CompletedTask;
        }
    }
}
