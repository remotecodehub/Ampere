namespace Ampere.Application.SignalR.Responses;

/// <summary>Describes live electrical telemetry.</summary>
public sealed record TelemetryResponse(
    string HouseId,
    string SonoffId,
    string EndpointId,
    DateTimeOffset MeasuredAt,
    decimal Voltage,
    decimal Current,
    decimal Power,
    decimal EnergyWh,
    bool RelayState);
