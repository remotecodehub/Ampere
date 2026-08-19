namespace Ampere.Application.SignalR.Requests;

/// <summary>Requests the latest telemetry snapshot.</summary>
/// <param name="HouseId">An optional house filter.</param>
public sealed record GetTelemetrySnapshotRequest(
    string? HouseId);
