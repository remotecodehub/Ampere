namespace Ampere.Application.SignalR.Requests;

/// <summary>Requests a live telemetry stream.</summary>
/// <param name="HouseId">An optional house filter.</param>
public sealed record WatchTelemetryRequest(
    string? HouseId);
