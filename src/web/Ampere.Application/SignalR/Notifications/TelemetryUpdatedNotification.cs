using Ampere.Application.SignalR.Responses;
using Mediator;

namespace Ampere.Application.SignalR.Notifications;

/// <summary>Publishes a new telemetry sample.</summary>
public sealed record TelemetryUpdatedNotification(
    TelemetryResponse Telemetry)
    : INotification;
