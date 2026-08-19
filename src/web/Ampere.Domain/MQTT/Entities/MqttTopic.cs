using Ampere.Domain.Common;

namespace Ampere.Domain.MQTT.Entities;

/// <summary>Represents a configured MQTT topic.</summary>
public sealed class MqttTopic : EntityBase
{
    /// <summary>Gets or sets the topic name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the topic description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets whether the topic is enabled.</summary>
    public bool Enabled { get; set; } = true;
}
