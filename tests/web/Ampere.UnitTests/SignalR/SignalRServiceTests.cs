using Ampere.Application.Common.Contracts;
using Ampere.Application.SignalR.Responses;
using Ampere.Infrastructure.Common.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Ampere.UnitTests.SignalR;

/// <summary>Tests the concrete SignalR infrastructure service.</summary>
public sealed class SignalRServiceTests
{
    [Fact]
    public async Task NotifyDiscovery_PublishesToAllClients()
    {
        (SignalRService service, Mock<IAmpereSignalRClient> client) =
            CreateService();
        DiscoveryResponse response =
            new("house", 30, true);

        await service.NotifyDiscoveryAsync(
            response,
            CancellationToken.None);

        client.Verify(item => item.DiscoveryStateChanged(response),
            Times.Once);
    }

    [Fact]
    public async Task NotifyRelay_PublishesToAllClients()
    {
        (SignalRService service, Mock<IAmpereSignalRClient> client) =
            CreateService();
        RelayStateResponse response =
            new("endpoint", true);

        await service.NotifyRelayStateAsync(
            response,
            CancellationToken.None);

        client.Verify(item => item.RelayStateChanged(response),
            Times.Once);
    }

    [Fact]
    public async Task NotifyFirmware_PublishesToAllClients()
    {
        (SignalRService service, Mock<IAmpereSignalRClient> client) =
            CreateService();
        FirmwareProgressResponse response =
            new("node", 25, "updating");

        await service.NotifyFirmwareProgressAsync(
            response,
            CancellationToken.None);

        client.Verify(item => item.FirmwareUpdateProgress(response),
            Times.Once);
    }

    [Fact]
    public async Task PublishTelemetry_UpdatesSnapshotAndClients()
    {
        (SignalRService service, Mock<IAmpereSignalRClient> client) =
            CreateService();
        TelemetryResponse response = CreateTelemetry(
            "house",
            "endpoint");

        await service.PublishTelemetryAsync(
            response,
            CancellationToken.None);

        IReadOnlyList<TelemetryResponse> snapshot =
            await service.GetTelemetrySnapshotAsync(
                null,
                CancellationToken.None);

        Assert.Single(snapshot);
        Assert.Same(response, snapshot[0]);
        client.Verify(item => item.TelemetryUpdated(response),
            Times.Once);
    }

    [Fact]
    public async Task PublishTelemetry_ReplacesLatestByEndpoint()
    {
        (SignalRService service, _) = CreateService();
        TelemetryResponse first = CreateTelemetry(
            "house",
            "endpoint");
        TelemetryResponse second = first with
        {
            EnergyWh = 200
        };

        await service.PublishTelemetryAsync(
            first,
            CancellationToken.None);
        await service.PublishTelemetryAsync(
            second,
            CancellationToken.None);

        IReadOnlyList<TelemetryResponse> snapshot =
            await service.GetTelemetrySnapshotAsync(
                null,
                CancellationToken.None);

        Assert.Single(snapshot);
        Assert.Equal(200, snapshot[0].EnergyWh);
    }

    [Fact]
    public async Task PublishTelemetry_CancellationStopsPublish()
    {
        (SignalRService service, Mock<IAmpereSignalRClient> client) =
            CreateService();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(
            async () => await service.PublishTelemetryAsync(
                CreateTelemetry("house", "endpoint"),
                cancellation.Token));

        client.Verify(item => item.TelemetryUpdated(
                It.IsAny<TelemetryResponse>()),
            Times.Never);
    }

    [Fact]
    public async Task Snapshot_FiltersHouseCaseInsensitively()
    {
        (SignalRService service, _) = CreateService();
        await service.PublishTelemetryAsync(
            CreateTelemetry("house-a", "endpoint-a"),
            CancellationToken.None);
        await service.PublishTelemetryAsync(
            CreateTelemetry("house-b", "endpoint-b"),
            CancellationToken.None);

        IReadOnlyList<TelemetryResponse> snapshot =
            await service.GetTelemetrySnapshotAsync(
                "HOUSE-A",
                CancellationToken.None);

        Assert.Single(snapshot);
        Assert.Equal("house-a", snapshot[0].HouseId);
    }

    [Fact]
    public async Task Snapshot_NullHouseReturnsAllEndpoints()
    {
        (SignalRService service, _) = CreateService();
        await service.PublishTelemetryAsync(
            CreateTelemetry("house-a", "endpoint-a"),
            CancellationToken.None);
        await service.PublishTelemetryAsync(
            CreateTelemetry("house-b", "endpoint-b"),
            CancellationToken.None);

        IReadOnlyList<TelemetryResponse> snapshot =
            await service.GetTelemetrySnapshotAsync(
                null,
                CancellationToken.None);

        Assert.Equal(2, snapshot.Count);
    }

    [Fact]
    public async Task WatchTelemetry_ReturnsMatchingHouse()
    {
        (SignalRService service, _) = CreateService();
        using CancellationTokenSource cancellation = new();
        await service.PublishTelemetryAsync(
            CreateTelemetry("house", "endpoint"),
            CancellationToken.None);

        await using IAsyncEnumerator<TelemetryResponse> enumerator =
            service.WatchTelemetryAsync(
                    "HOUSE",
                    cancellation.Token)
                .GetAsyncEnumerator(cancellation.Token);

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("endpoint", enumerator.Current.EndpointId);
        cancellation.Cancel();
    }

    [Fact]
    public async Task WatchTelemetry_NullHouseReturnsAll()
    {
        (SignalRService service, _) = CreateService();
        using CancellationTokenSource cancellation = new();
        await service.PublishTelemetryAsync(
            CreateTelemetry("house", "endpoint"),
            CancellationToken.None);

        await using IAsyncEnumerator<TelemetryResponse> enumerator =
            service.WatchTelemetryAsync(
                    null,
                    cancellation.Token)
                .GetAsyncEnumerator(cancellation.Token);

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("house", enumerator.Current.HouseId);
        cancellation.Cancel();
    }

    [Fact]
    public async Task WatchTelemetry_SkipsOtherHouses()
    {
        (SignalRService service, _) = CreateService();
        using CancellationTokenSource cancellation = new();
        await service.PublishTelemetryAsync(
            CreateTelemetry("other", "other-endpoint"),
            CancellationToken.None);
        await service.PublishTelemetryAsync(
            CreateTelemetry("house", "endpoint"),
            CancellationToken.None);

        await using IAsyncEnumerator<TelemetryResponse> enumerator =
            service.WatchTelemetryAsync(
                    "house",
                    cancellation.Token)
                .GetAsyncEnumerator(cancellation.Token);

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("endpoint", enumerator.Current.EndpointId);
        cancellation.Cancel();
    }

    [Fact]
    public async Task WatchTelemetry_CancellationStopsStream()
    {
        (SignalRService service, _) = CreateService();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await using IAsyncEnumerator<TelemetryResponse> enumerator =
            service.WatchTelemetryAsync(
                    null,
                    cancellation.Token)
                .GetAsyncEnumerator(cancellation.Token);

        await Assert.ThrowsAsync<TaskCanceledException>(
            async () => await enumerator.MoveNextAsync());
    }

    private static (SignalRService, Mock<IAmpereSignalRClient>)
        CreateService()
    {
        Mock<IAmpereSignalRClient> client = new();
        Mock<IHubClients<IAmpereSignalRClient>> clients = new();
        clients.Setup(item => item.All)
            .Returns(client.Object);
        Mock<IHubContext<Hub<IAmpereSignalRClient>,
            IAmpereSignalRClient>> context = new();
        context.Setup(item => item.Clients)
            .Returns(clients.Object);

        return (new SignalRService(context.Object), client);
    }

    private static TelemetryResponse CreateTelemetry(
        string houseId,
        string endpointId)
    {
        return new TelemetryResponse(
            houseId,
            "node",
            endpointId,
            DateTimeOffset.UtcNow,
            220,
            1,
            220,
            100,
            true);
    }
}
