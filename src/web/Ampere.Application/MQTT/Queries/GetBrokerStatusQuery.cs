using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Queries;

/// <summary>Gets broker runtime status.</summary>
public sealed record GetBrokerStatusQuery
    : IRequest<BrokerStatusResponse>;
