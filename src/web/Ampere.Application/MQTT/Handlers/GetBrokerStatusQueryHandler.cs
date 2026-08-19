using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Gets broker runtime status.</summary>
/// <param name="brokerService">The broker service.</param>
public sealed class GetBrokerStatusQueryHandler(
    IMqttBrokerService brokerService)
    : IRequestHandler<GetBrokerStatusQuery,
        BrokerStatusResponse>
{
    /// <inheritdoc />
    public ValueTask<BrokerStatusResponse> Handle(
        GetBrokerStatusQuery request,
        CancellationToken cancellationToken)
    {
        return new ValueTask<BrokerStatusResponse>(
            brokerService.GetStatusAsync(
                cancellationToken));
    }
}
