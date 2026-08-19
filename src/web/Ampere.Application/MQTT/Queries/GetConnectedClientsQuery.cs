using Mediator;

namespace Ampere.Application.MQTT.Queries;

/// <summary>Requests the currently connected clients.</summary>
public sealed record GetConnectedClientsQuery
    : IRequest<IReadOnlyList<Responses.MqttClientResponse>>;
