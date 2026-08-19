namespace Ampere.Infrastructure.MQTT.Models;

/// <summary>
/// Persistence model for broker configuration adapted to EF.
/// </summary>
public sealed class MqttBrokerConfigurationEntity
{
    public Guid Id { get; set; }
    public string? BindAddress { get; set; }
    public int Port { get; set; }
    public bool StartOnBoot { get; set; }
    public bool UseTls { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
