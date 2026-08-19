using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Responses;
using Mediator.Net.Context;
using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Stops the broker using the configured service.</summary>
public sealed class StopBrokerCommandHandler(IMqttBrokerService brokerService) : IRequestHandler<StopBrokerCommand, Ampere.Application.Common.Responses.Response<Ampere.Application.MQTT.Responses.BrokerStatusResponse>>
{
    public async Task<Ampere.Application.Common.Responses.Response<Ampere.Application.MQTT.Responses.BrokerStatusResponse>> Handle(IReceiveContext<StopBrokerCommand> context, CancellationToken cancellationToken)
    {
        await brokerService.StopAsync(cancellationToken);
        var status = await brokerService.GetStatusAsync(cancellationToken);
        return Ampere.Application.Common.Responses.Response.Success(status);
    }
}
