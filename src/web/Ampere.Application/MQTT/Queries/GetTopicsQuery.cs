using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Queries;

/// <summary>Gets configured MQTT topics.</summary>
public sealed record GetTopicsQuery
    : IRequest<IReadOnlyList<MqttTopicResponse>>;
