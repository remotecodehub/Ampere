namespace Ampere.Application.MQTT.Requests;

/// <summary>
/// Represents the HTTP/application payload to configure
/// the MQTT broker persistently.
/// </summary>
public sealed record ConfigureBrokerRequest(
    string? BindAddress,
    int Port,
    bool StartOnBoot,
    bool UseTls);
