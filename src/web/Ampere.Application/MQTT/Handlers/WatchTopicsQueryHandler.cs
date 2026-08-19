using System.Runtime.CompilerServices;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Streams MQTT messages.</summary>
/// <param name="brokerService">The broker service.</param>
public sealed class WatchTopicsQueryHandler(
    IMqttBrokerService brokerService)
    : IStreamRequestHandler<WatchTopicsQuery,
        MqttTopicMessageResponse>
{
    /// <inheritdoc />
    public async IAsyncEnumerable<MqttTopicMessageResponse>
        Handle(
            WatchTopicsQuery request,
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
    {
        await foreach (MqttTopicMessageResponse message
            in brokerService.WatchMessagesAsync(
                cancellationToken))
        {
            yield return message;
        }
    }
}
