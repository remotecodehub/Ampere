using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Requests;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Publishes a message through MQTT.</summary>
/// <param name="brokerService">The broker service.</param>
public sealed class PublishMessageCommandHandler(
    IMqttBrokerService brokerService)
    : IRequestHandler<PublishMessageCommand,
        Response<bool>>
{
    /// <inheritdoc />
    public async ValueTask<Response<bool>> Handle(
        PublishMessageCommand request,
        CancellationToken cancellationToken)
    {
        PublishMessageRequest publish = request.Request;

        try
        {
            await brokerService.PublishAsync(
                publish.Topic,
                publish.Payload,
                cancellationToken);
            return Response.Success(true);
        }
        catch (InvalidOperationException exception)
        {
            return Response.Failure<bool>(
                [exception.Message]);
        }
    }
}
