using Ampere.Application.SignalR.Responses;
using Mediator;

namespace Ampere.Application.SignalR.Queries;

/// <summary>Streams live telemetry notifications.</summary>
public sealed record WatchTelemetryQuery(
    string? HouseId)
    : IStreamRequest<TelemetryResponse>;
