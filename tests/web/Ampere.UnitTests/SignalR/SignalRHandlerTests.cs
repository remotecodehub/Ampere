using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Commands;
using Ampere.Application.SignalR.Handlers;
using Ampere.Application.SignalR.Notifications;
using Ampere.Application.SignalR.Responses;
using Xunit;

namespace Ampere.UnitTests.SignalR;

/// <summary>Tests SignalR Mediator handlers.</summary>
public sealed class SignalRHandlerTests
{
    [Fact]
    public async Task StartDiscovery_NotifiesHub()
    {
        FakeSignalRService service = new();
        SignalRHandlers handler = new(service);

        Response<DiscoveryResponse> result =
            await handler.Handle(
                new StartDiscoveryCommand("house", 60),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("house", result.Data?.HouseId);
        Assert.Equal(60, result.Data?.WindowSeconds);
    }

    [Fact]
    public async Task SetRelay_NotifiesHub()
    {
        FakeSignalRService service = new();
        SignalRHandlers handler = new(service);

        Response<RelayStateResponse> result =
            await handler.Handle(
                new SetRelayCommand("endpoint", true),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(result.Data?.State);
        Assert.True(service.Relay?.State);
    }

    [Fact]
    public async Task FirmwareUpdate_NotifiesHub()
    {
        FakeSignalRService service = new();
        SignalRHandlers handler = new(service);

        Response<FirmwareProgressResponse> result =
            await handler.Handle(
                new StartFirmwareUpdateCommand("node", "2.0.0"),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Data?.Percent);
        Assert.Equal("node", service.Firmware?.NodeId);
    }

    [Fact]
    public async Task TelemetryNotification_PublishesToHub()
    {
        FakeSignalRService service = new();
        TelemetryNotificationHandler handler = new(service);
        TelemetryResponse response = new(
            "house",
            "node",
            "endpoint",
            DateTimeOffset.UtcNow,
            220,
            1,
            220,
            100,
            true);

        await handler.Handle(
            new TelemetryUpdatedNotification(response),
            CancellationToken.None);

        Assert.Same(response, service.Telemetry);
    }

    private sealed class FakeSignalRService : ISignalRService
    {
        public DiscoveryResponse? Discovery { get; private set; }
        public RelayStateResponse? Relay { get; private set; }
        public FirmwareProgressResponse? Firmware { get; private set; }
        public TelemetryResponse? Telemetry { get; private set; }

        public Task NotifyDiscoveryAsync(
            DiscoveryResponse response,
            CancellationToken cancellationToken)
        {
            Discovery = response;
            return Task.CompletedTask;
        }

        public Task NotifyRelayStateAsync(
            RelayStateResponse response,
            CancellationToken cancellationToken)
        {
            Relay = response;
            return Task.CompletedTask;
        }

        public Task NotifyFirmwareProgressAsync(
            FirmwareProgressResponse response,
            CancellationToken cancellationToken)
        {
            Firmware = response;
            return Task.CompletedTask;
        }

        public Task PublishTelemetryAsync(
            TelemetryResponse response,
            CancellationToken cancellationToken)
        {
            Telemetry = response;
            return Task.CompletedTask;
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
