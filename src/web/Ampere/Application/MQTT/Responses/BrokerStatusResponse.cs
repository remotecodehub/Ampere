namespace Ampere.Application.MQTT.Responses;

/// <summary>
/// Basic runtime status for the MQTT broker.
/// </summary>
public sealed record BrokerStatusResponse(
    bool IsRunning,
    DateTimeOffset? StartedAt,
    int Port,
    string? BindAddress,
    int ConnectedClientsCount);
