namespace Ampere.Application.MQTT.Results;

/// <summary>
/// Response containing stored broker configuration.
/// </summary>
public sealed record BrokerConfigurationResult(
    string Id,
    string? BindAddress,
    int Port,
    bool StartOnBoot,
    bool UseTls,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
