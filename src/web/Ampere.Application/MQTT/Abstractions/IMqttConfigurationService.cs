using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;

namespace Ampere.Application.MQTT.Abstractions;

/// <summary>
/// Abstraction for persisting and retrieving broker
/// configuration without depending on infrastructure.
/// </summary>
public interface IMqttConfigurationService
{
    /// <summary>Gets the last persisted configuration or
    /// <see langword="null"/> when none exists.</summary>
    Task<BrokerConfigurationResponse?> GetConfigurationAsync(CancellationToken cancellationToken);

    /// <summary>Saves the supplied configuration and returns
    /// the stored representation.</summary>
    Task<BrokerConfigurationResponse> SaveConfigurationAsync(ConfigureBrokerRequest request, CancellationToken cancellationToken);
}
