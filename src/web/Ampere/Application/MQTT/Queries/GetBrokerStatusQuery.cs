using Ampere.Application.MQTT.Responses;
using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Queries;

/// <summary>Query to obtain current broker runtime status.</summary>
public sealed record GetBrokerStatusQuery() : IRequest;
