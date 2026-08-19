using Ampere.Domain.Common;

namespace Ampere.Infrastructure.MQTT.Models;

/// <summary>
/// Stores the persisted MQTT broker settings.
/// </summary>
public sealed class MqttBrokerConfigurationEntity
    : EntityBase
{
    /// <summary>
    /// Gets or sets the broker bind address.
    /// </summary>
    public string? BindAddress { get; set; }

    /// <summary>
    /// Gets or sets the broker port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets whether the broker starts on boot.
    /// </summary>
    public bool StartOnBoot { get; set; }

    /// <summary>
    /// Gets or sets whether TLS is enabled.
    /// </summary>
    public bool UseTls { get; set; }
}
