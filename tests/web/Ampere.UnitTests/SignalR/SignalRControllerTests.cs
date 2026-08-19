using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Commands;
using Ampere.Application.SignalR.Queries;
using Ampere.Application.SignalR.Requests;
using Ampere.Application.SignalR.Responses;
using Ampere.Controllers.v1;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ampere.UnitTests.SignalR;

/// <summary>Tests SignalR presentation dispatch.</summary>
public sealed class SignalRControllerTests
{
    [Fact]
    public async Task StartDiscovery_DispatchesCommand()
    {
        Mock<IMediator> mediator = new();
        DiscoveryResponse response = new("house", 60);
        mediator.Setup(item => item.Send(
                It.IsAny<StartDiscoveryCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Response<DiscoveryResponse>>(
                Response.Success(response)));
        SignalRController controller =
            new(mediator.Object);

        IActionResult result = await controller.StartDiscovery(
            new StartDiscoveryRequest("house", 60),
            CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response,
            Assert.IsType<Response<DiscoveryResponse>>(
                ok.Value).Data);
        mediator.Verify(item => item.Send(
                It.Is<StartDiscoveryCommand>(command =>
                    command.HouseId == "house" &&
                    command.WindowSeconds == 60),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartDiscovery_Failure_ReturnsBadRequest()
    {
        Mock<IMediator> mediator = new();
        Response<DiscoveryResponse> response =
            Response.Failure<DiscoveryResponse>("failure");
        mediator.Setup(item => item.Send(
                It.IsAny<StartDiscoveryCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Response<DiscoveryResponse>>(
                response));
        SignalRController controller =
            new(mediator.Object);

        IActionResult result = await controller.StartDiscovery(
            new StartDiscoveryRequest("house", 60),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SetRelay_DispatchesCommand()
    {
        Mock<IMediator> mediator = new();
        mediator.Setup(item => item.Send(
                It.IsAny<SetRelayCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Response<RelayStateResponse>>(
                Response.Success(new RelayStateResponse(
                    "endpoint",
                    true))));
        SignalRController controller =
            new(mediator.Object);

        IActionResult result = await controller.SetRelay(
            new SetRelayRequest("endpoint", true),
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        mediator.Verify(item => item.Send(
                It.Is<SetRelayCommand>(command =>
                    command.EndpointId == "endpoint" &&
                    command.State),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetRelay_Failure_ReturnsBadRequest()
    {
        Mock<IMediator> mediator = new();
        mediator.Setup(item => item.Send(
                It.IsAny<SetRelayCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Response<RelayStateResponse>>(
                Response.Failure<RelayStateResponse>("failure")));
        SignalRController controller =
            new(mediator.Object);

        IActionResult result = await controller.SetRelay(
            new SetRelayRequest("endpoint", false),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Firmware_DispatchesCommand()
    {
        Mock<IMediator> mediator = new();
        mediator.Setup(item => item.Send(
                It.IsAny<StartFirmwareUpdateCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Response<FirmwareProgressResponse>>(
                Response.Success(new FirmwareProgressResponse(
                    "node",
                    "2.0.0",
                    5))));
        SignalRController controller =
            new(mediator.Object);

        IActionResult result = await controller.StartFirmwareUpdate(
            new FirmwareUpdateRequest("node", "2.0.0"),
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        mediator.Verify(item => item.Send(
                It.Is<StartFirmwareUpdateCommand>(command =>
                    command.NodeId == "node" &&
                    command.Version == "2.0.0"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Firmware_Failure_ReturnsBadRequest()
    {
        Mock<IMediator> mediator = new();
        mediator.Setup(item => item.Send(
                It.IsAny<StartFirmwareUpdateCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Response<FirmwareProgressResponse>>(
                Response.Failure<FirmwareProgressResponse>("failure")));
        SignalRController controller =
            new(mediator.Object);

        IActionResult result = await controller.StartFirmwareUpdate(
            new FirmwareUpdateRequest("node", "2.0.0"),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Snapshot_DispatchesQuery()
    {
        Mock<IMediator> mediator = new();
        TelemetrySnapshotResponse response =
            new([new TelemetryResponse(
                "house",
                "node",
                "endpoint",
                DateTimeOffset.UtcNow,
                220,
                1,
                220,
                100,
                true)]);
        mediator.Setup(item => item.Send(
                It.IsAny<GetTelemetrySnapshotQuery>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Response<TelemetrySnapshotResponse>>(
                Response.Success(response)));
        SignalRController controller =
            new(mediator.Object);

        IActionResult result = await controller.GetSnapshot(
            new GetTelemetrySnapshotRequest("house"),
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        mediator.Verify(item => item.Send(
                It.Is<GetTelemetrySnapshotQuery>(query =>
                    query.HouseId == "house"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Snapshot_PropagatesFailureResponse()
    {
        Mock<IMediator> mediator = new();
        Response<TelemetrySnapshotResponse> response =
            Response.Failure<TelemetrySnapshotResponse>("failure");
        mediator.Setup(item => item.Send(
                It.IsAny<GetTelemetrySnapshotQuery>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Response<TelemetrySnapshotResponse>>(
                response));
        SignalRController controller =
            new(mediator.Object);

        IActionResult result = await controller.GetSnapshot(
            new GetTelemetrySnapshotRequest(null),
            CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task WatchTelemetry_DispatchesStream()
    {
        Mock<IMediator> mediator = new();
        List<TelemetryResponse> expected =
        [
            new(
                "house",
                "node",
                "endpoint",
                DateTimeOffset.UtcNow,
                220,
                1,
                220,
                100,
                true)
        ];
        mediator.Setup(item => item.CreateStream(
                It.IsAny<WatchTelemetryQuery>(),
                It.IsAny<CancellationToken>()))
            .Returns(Stream(expected));
        SignalRController controller =
            new(mediator.Object);

        IAsyncEnumerable<TelemetryResponse> result =
            controller.WatchTelemetry(
                new WatchTelemetryRequest("house"),
                CancellationToken.None);
        List<TelemetryResponse> actual = [];
        await foreach (TelemetryResponse item in result)
        {
            actual.Add(item);
        }

        Assert.Single(actual);
        mediator.Verify(item => item.CreateStream(
                It.Is<WatchTelemetryQuery>(query =>
                    query.HouseId == "house"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static async IAsyncEnumerable<TelemetryResponse>
        Stream(IEnumerable<TelemetryResponse> responses)
    {
        foreach (TelemetryResponse response in responses)
        {
            await Task.Yield();
            yield return response;
        }
    }
}
