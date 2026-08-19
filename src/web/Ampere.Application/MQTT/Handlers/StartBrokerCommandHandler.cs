using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Starts the MQTT broker.</summary>
/// <param name="brokerService">The broker service.</param>
public sealed class StartBrokerCommandHandler(
    IMqttBrokerService brokerService)
    : IRequestHandler<StartBrokerCommand,
        BrokerStatusResponse>
{
    /// <inheritdoc />
    public async ValueTask<BrokerStatusResponse> Handle(
        StartBrokerCommand request,
        CancellationToken cancellationToken)
    {
        await brokerService.StartAsync(
            cancellationToken);
        return await brokerService.GetStatusAsync(
            cancellationToken);
    }
}
