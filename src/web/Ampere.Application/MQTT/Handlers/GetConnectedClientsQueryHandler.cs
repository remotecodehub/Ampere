using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Gets connected MQTT clients.</summary>
/// <param name="brokerService">The broker service.</param>
public sealed class GetConnectedClientsQueryHandler(
    IMqttBrokerService brokerService)
    : IRequestHandler<GetConnectedClientsQuery,
        IReadOnlyList<MqttClientResponse>>
{
    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<MqttClientResponse>>
        Handle(
            GetConnectedClientsQuery request,
            CancellationToken cancellationToken)
    {
        return await brokerService.GetClientsAsync(
            cancellationToken);
    }
}
