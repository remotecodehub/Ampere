using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Responses;
using Mediator;

namespace Ampere.Application.SignalR.Queries;

/// <summary>Gets the latest telemetry snapshot.</summary>
public sealed record GetTelemetrySnapshotQuery(
    string? HouseId)
    : IRequest<Response<TelemetrySnapshotResponse>>;
