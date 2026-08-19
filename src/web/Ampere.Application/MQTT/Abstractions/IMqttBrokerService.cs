using Ampere.Application.MQTT.Responses;

namespace Ampere.Application.MQTT.Abstractions;

/// <summary>Controls the local MQTT broker runtime.</summary>
public interface IMqttBrokerService
{
    /// <summary>Starts the configured broker.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>Stops the broker.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>Restarts the broker.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RestartAsync(CancellationToken cancellationToken);

    /// <summary>Gets broker runtime status.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The broker status.</returns>
    Task<BrokerStatusResponse> GetStatusAsync(
        CancellationToken cancellationToken);

    /// <summary>Gets connected MQTT clients.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connected clients.</returns>
    Task<IReadOnlyList<MqttClientResponse>>
        GetClientsAsync(CancellationToken cancellationToken);

    /// <summary>Publishes a message to the broker.</summary>
    /// <param name="topic">The topic name.</param>
    /// <param name="payload">The message payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync(
        string topic,
        byte[] payload,
        CancellationToken cancellationToken);

    /// <summary>Streams messages received by the broker.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The live message stream.</returns>
    IAsyncEnumerable<MqttTopicMessageResponse>
        WatchMessagesAsync(CancellationToken cancellationToken);
}
