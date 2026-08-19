namespace Ampere.Application.MQTT.Responses;

/// <summary>
/// Response containing stored broker configuration.
/// </summary>
public sealed record BrokerConfigurationResponse(
    string Id,
    string? BindAddress,
    int Port,
    bool StartOnBoot,
    bool UseTls,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
    
{
    public MQTT.Results.BrokerConfigurationResult ToResult()
        => new (Id, BindAddress, Port, StartOnBoot, UseTls, CreatedAt, UpdatedAt);
}
