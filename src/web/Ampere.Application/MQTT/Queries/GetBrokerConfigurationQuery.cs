using Ampere.Application.MQTT.Responses;
using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Queries;

/// <summary>Query for the last saved broker configuration.</summary>
public sealed record GetBrokerConfigurationQuery() : IRequest;
