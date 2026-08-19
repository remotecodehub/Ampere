namespace Ampere.Application.MQTT.Requests;

/// <summary>
/// Request to publish a message to a topic.
/// </summary>
public sealed record PublishMessageRequest(string Topic, byte[] Payload, bool Retain = false, int QoS = 0);
