namespace Ampere.Application.SignalR.Responses;

/// <summary>Contains the latest endpoint telemetry.</summary>
public sealed record TelemetrySnapshotResponse(
    IReadOnlyList<TelemetryResponse> Items);
