using Ampere.Application.Common.Abstractions;
using Ampere.Application.SignalR.Notifications;
using Mediator;

namespace Ampere.Application.SignalR.Handlers;

/// <summary>Handles telemetry notifications.</summary>
/// <param name="signalRService">The hub service.</param>
public sealed class TelemetryNotificationHandler(
    ISignalRService signalRService)
    : INotificationHandler<TelemetryUpdatedNotification>
{
    /// <inheritdoc />
    public ValueTask Handle(
        TelemetryUpdatedNotification notification,
        CancellationToken cancellationToken)
    {
        return new ValueTask(
            signalRService.PublishTelemetryAsync(
                notification.Telemetry,
                cancellationToken));
    }
}
