namespace Ampere.Application.MQTT.Results;

/// <summary>
/// Basic runtime status for the MQTT broker.
/// </summary>
public sealed record BrokerStatusResult(
    bool IsRunning,
    DateTimeOffset? StartedAt,
    int Port,
    string? BindAddress,
    int ConnectedClientsCount);
