using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Stops the MQTT broker.</summary>
/// <param name="brokerService">The broker service.</param>
public sealed class StopBrokerCommandHandler(
    IMqttBrokerService brokerService)
    : IRequestHandler<StopBrokerCommand,
        BrokerStatusResponse>
{
    /// <inheritdoc />
    public async ValueTask<BrokerStatusResponse> Handle(
        StopBrokerCommand request,
        CancellationToken cancellationToken)
    {
        await brokerService.StopAsync(
            cancellationToken);
        return await brokerService.GetStatusAsync(
            cancellationToken);
    }
}
