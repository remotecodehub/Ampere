using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Commands;
using Ampere.Application.SignalR.Responses;
using Mediator;

namespace Ampere.Application.SignalR.Handlers;

/// <summary>Handles SignalR commands.</summary>
/// <param name="signalRService">The hub service.</param>
public sealed class SignalRHandlers(
    ISignalRService signalRService)
    : IRequestHandler<StartDiscoveryCommand,
        Response<DiscoveryResponse>>,
      IRequestHandler<SetRelayCommand,
        Response<RelayStateResponse>>,
      IRequestHandler<StartFirmwareUpdateCommand,
        Response<FirmwareProgressResponse>>
{
    /// <inheritdoc />
    public async ValueTask<
        Response<DiscoveryResponse>> Handle(
            StartDiscoveryCommand request,
            CancellationToken cancellationToken)
    {
        DiscoveryResponse response = new(
            request.HouseId,
            request.WindowSeconds,
            true);

        await signalRService.NotifyDiscoveryAsync(
            response,
            cancellationToken);
        return Response.Success(response);
    }

    /// <inheritdoc />
    public async ValueTask<
        Response<RelayStateResponse>> Handle(
            SetRelayCommand request,
            CancellationToken cancellationToken)
    {
        RelayStateResponse response = new(
            request.EndpointId,
            request.State);

        await signalRService.NotifyRelayStateAsync(
            response,
            cancellationToken);
        return Response.Success(response);
    }

    /// <inheritdoc />
    public async ValueTask<
        Response<FirmwareProgressResponse>> Handle(
            StartFirmwareUpdateCommand request,
            CancellationToken cancellationToken)
    {
        FirmwareProgressResponse response = new(
            request.NodeId,
            5,
            "Starting firmware update.");

        await signalRService.NotifyFirmwareProgressAsync(
            response,
            cancellationToken);
        return Response.Success(response);
    }
}
