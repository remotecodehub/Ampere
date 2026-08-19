namespace Ampere.Application.MQTT.Responses;

/// <summary>Describes a configured MQTT topic.</summary>
/// <param name="Id">The topic identifier.</param>
/// <param name="Name">The MQTT topic name.</param>
/// <param name="Description">The description.</param>
/// <param name="Enabled">Whether the topic is enabled.</param>
public sealed record MqttTopicResponse(
    string Id,
    string Name,
    string? Description,
    bool Enabled);
