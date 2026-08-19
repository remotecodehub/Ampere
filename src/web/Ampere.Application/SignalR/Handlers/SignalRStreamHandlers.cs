using Ampere.Application.Common.Abstractions;
using Ampere.Application.SignalR.Queries;
using Ampere.Application.SignalR.Responses;
using Mediator;

namespace Ampere.Application.SignalR.Handlers;

/// <summary>Handles live telemetry streams.</summary>
/// <param name="signalRService">The hub service.</param>
public sealed class SignalRStreamHandlers(
    ISignalRService signalRService)
    : IStreamRequestHandler<WatchTelemetryQuery,
        TelemetryResponse>
{
    /// <inheritdoc />
    public IAsyncEnumerable<TelemetryResponse> Handle(
        WatchTelemetryQuery request,
        CancellationToken cancellationToken)
    {
        return signalRService.WatchTelemetryAsync(
            request.HouseId,
            cancellationToken);
    }
}
