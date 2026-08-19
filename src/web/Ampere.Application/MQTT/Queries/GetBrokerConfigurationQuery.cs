using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Queries;

/// <summary>Gets persisted broker configuration.</summary>
public sealed record GetBrokerConfigurationQuery
    : IRequest<Response<BrokerConfigurationResponse?>>;
