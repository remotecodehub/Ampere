namespace Ampere.Application.MQTT.Responses;

/// <summary>Describes a connected MQTT client.</summary>
/// <param name="ClientId">The MQTT client identifier.</param>
/// <param name="UserName">The authenticated user name.</param>
/// <param name="ConnectedAt">The connection timestamp.</param>
/// <param name="Endpoint">The remote endpoint.</param>
public sealed record MqttClientResponse(
    string ClientId,
    string? UserName,
    DateTimeOffset? ConnectedAt,
    string? Endpoint);
