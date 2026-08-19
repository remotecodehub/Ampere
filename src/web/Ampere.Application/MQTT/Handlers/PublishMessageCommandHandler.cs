using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Requests;
using Mediator.Net.Context;
using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Publishes a message through the MQTT broker service.</summary>
public sealed class PublishMessageCommandHandler(IMqttBrokerService brokerService) : IRequestHandler<PublishMessageCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(IReceiveContext<PublishMessageCommand> context, CancellationToken cancellationToken)
    {
        PublishMessageRequest req = context.Message.Request;

        try
        {
            await brokerService.PublishAsync(req.Topic, req.Payload, cancellationToken);
            return Response.Success(true);
        }
        catch (Exception ex)
        {
            return Response.Failure<bool>(ex.Message);
        }
    }
}
