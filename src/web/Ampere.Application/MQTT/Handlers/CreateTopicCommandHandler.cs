using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Responses;
using Ampere.Domain.MQTT.Entities;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Creates configured MQTT topics.</summary>
/// <param name="repository">The topic repository.</param>
public sealed class CreateTopicCommandHandler(
    IRepository<MqttTopic> repository)
    : IRequestHandler<CreateTopicCommand,
        Response<MqttTopicResponse>>
{
    /// <inheritdoc />
    public async ValueTask<Response<MqttTopicResponse>> Handle(
        CreateTopicCommand request,
        CancellationToken cancellationToken)
    {
        MqttTopic? existing =
            await repository.FirstOrDefaultAsync(
                topic => topic.Name == request.Request.Name,
                [],
                cancellationToken);

        if (existing is not null)
        {
            return Response.Failure<MqttTopicResponse>(
                ["The MQTT topic already exists."]);
        }

        MqttTopic entity = new()
        {
            Name = request.Request.Name,
            Description = request.Request.Description
        };

        await repository.AddAsync(
            entity,
            cancellationToken);

        return Response.Success(
            new MqttTopicResponse(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Enabled));
    }
}
