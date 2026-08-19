namespace Ampere.Application.MQTT.Responses;

/// <summary>Represents a live MQTT message.</summary>
/// <param name="Topic">The topic name.</param>
/// <param name="Payload">The UTF-8 payload.</param>
/// <param name="ClientId">The publishing client.</param>
/// <param name="ReceivedAt">The reception timestamp.</param>
public sealed record MqttTopicMessageResponse(
    string Topic,
    string Payload,
    string? ClientId,
    DateTimeOffset ReceivedAt);
