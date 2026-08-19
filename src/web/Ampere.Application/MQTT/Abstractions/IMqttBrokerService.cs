using Ampere.Application.MQTT.Responses;

namespace Ampere.Application.MQTT.Abstractions;

/// <summary>
/// Abstraction for controlling the MQTT broker lifecycle and
/// operations without referencing MQTTnet types.
/// </summary>
public interface IMqttBrokerService
{
    /// <summary>Starts the MQTT broker using the current
    /// persisted configuration.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>Stops the MQTT broker.</summary>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>Restarts the MQTT broker.</summary>
    Task RestartAsync(CancellationToken cancellationToken);

    /// <summary>Gets information about the current broker
    /// status and runtime details.</summary>
    Task<BrokerStatusResponse> GetStatusAsync(CancellationToken cancellationToken);

    /// <summary>Publishes a message to the broker.
    /// </summary>
    Task PublishAsync(string topic, byte[] payload, CancellationToken cancellationToken);
}
