using Ampere.Application.Common.Abstractions;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Responses;
using Ampere.Domain.MQTT.Entities;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Gets configured MQTT topics.</summary>
/// <param name="repository">The topic repository.</param>
public sealed class GetTopicsQueryHandler(
    IRepository<MqttTopic> repository)
    : IRequestHandler<GetTopicsQuery,
        IReadOnlyList<MqttTopicResponse>>
{
    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<MqttTopicResponse>>
        Handle(
            GetTopicsQuery request,
            CancellationToken cancellationToken)
    {
        IReadOnlyList<MqttTopic> topics =
            await repository.ListAsync(
                null,
                [],
                cancellationToken);

        return topics
            .Select(topic => new MqttTopicResponse(
                topic.Id,
                topic.Name,
                topic.Description,
                topic.Enabled))
            .ToArray();
    }
}
