namespace Ampere.Application.MQTT.Requests;

/// <summary>Defines a topic to persist.</summary>
/// <param name="Name">The MQTT topic name.</param>
/// <param name="Description">The topic description.</param>
public sealed record CreateTopicRequest(
    string Name,
    string? Description);
