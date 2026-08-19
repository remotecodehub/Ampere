using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Commands;
using Ampere.Application.SignalR.Queries;
using Ampere.Application.SignalR.Requests;
using Ampere.Application.SignalR.Responses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ampere.Controllers.v1;

/// <summary>Exposes SignalR orchestration endpoints.</summary>
/// <param name="mediator">The application mediator.</param>
[ApiController]
[Route("api/v1/signalr")]
[Authorize]
public sealed class SignalRController(
    IMediator mediator) : ControllerBase
{
    /// <summary>Starts device discovery.</summary>
    [HttpPost("discovery/start")]
    [ProducesResponseType<
        Response<DiscoveryResponse>>(200)]
    public async Task<IActionResult> StartDiscovery(
        [FromBody] StartDiscoveryRequest request,
        CancellationToken cancellationToken)
    {
        Response<DiscoveryResponse> result =
            await mediator.Send(
                new StartDiscoveryCommand(
                    request.HouseId,
                    request.WindowSeconds),
                cancellationToken);
        return result.Succeeded
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Changes an endpoint relay state.</summary>
    [HttpPost("relay")]
    [ProducesResponseType<
        Response<RelayStateResponse>>(200)]
    public async Task<IActionResult> SetRelay(
        [FromBody] SetRelayRequest request,
        CancellationToken cancellationToken)
    {
        Response<RelayStateResponse> result =
            await mediator.Send(
                new SetRelayCommand(
                    request.EndpointId,
                    request.State),
                cancellationToken);
        return result.Succeeded
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Starts a firmware update flow.</summary>
    [HttpPost("firmware")]
    [ProducesResponseType<
        Response<FirmwareProgressResponse>>(200)]
    public async Task<IActionResult> StartFirmwareUpdate(
        [FromBody] FirmwareUpdateRequest request,
        CancellationToken cancellationToken)
    {
        Response<FirmwareProgressResponse> result =
            await mediator.Send(
                new StartFirmwareUpdateCommand(
                    request.NodeId,
                    request.Version),
                cancellationToken);
        return result.Succeeded
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Streams live telemetry.</summary>
    [HttpGet("telemetry")]
    [Produces("application/x-ndjson")]
    public IAsyncEnumerable<TelemetryResponse> WatchTelemetry(
        [FromQuery] WatchTelemetryRequest request,
        CancellationToken cancellationToken)
    {
        return mediator.CreateStream(
            new WatchTelemetryQuery(request.HouseId),
            cancellationToken);
    }
}
