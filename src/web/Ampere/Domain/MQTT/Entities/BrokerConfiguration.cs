namespace Ampere.Domain.MQTT.Entities;

/// <summary>
/// Represents persisted configuration for the MQTT broker.
/// </summary>
public sealed class BrokerConfiguration
{
    /// <summary>Primary identifier.</summary>
    public string Id { get; set; } = Guid.CreateVersion7().ToString();

    /// <summary>The network address to bind the broker to (e.g. 0.0.0.0).</summary>
    public string? BindAddress { get; set; }

    /// <summary>The TCP port the broker listens on.</summary>
    public int Port { get; set; }

    /// <summary>Whether to start the broker automatically on app start.</summary>
    public bool StartOnBoot { get; set; }

    /// <summary>Whether to enable TLS for the broker.</summary>
    public bool UseTls { get; set; }

    /// <summary>When the configuration was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the configuration was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
