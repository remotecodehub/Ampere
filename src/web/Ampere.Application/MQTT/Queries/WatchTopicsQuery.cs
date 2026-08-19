using Mediator;

namespace Ampere.Application.MQTT.Queries;

/// <summary>Streams messages published to the broker.</summary>
public sealed record WatchTopicsQuery
    : IStreamRequest<Responses.MqttTopicMessageResponse>;
