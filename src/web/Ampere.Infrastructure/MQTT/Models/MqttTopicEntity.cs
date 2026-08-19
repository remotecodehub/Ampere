using Ampere.Domain.Common;

namespace Ampere.Infrastructure.MQTT.Models;

/// <summary>Stores a configured MQTT topic.</summary>
public sealed class MqttTopicEntity : EntityBase
{
    /// <summary>Gets or sets the topic name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the topic description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets whether the topic is enabled.</summary>
    public bool Enabled { get; set; } = true;
}
