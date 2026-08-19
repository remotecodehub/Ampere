using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Starts the MQTT broker.</summary>
public sealed record StartBrokerCommand
    : IRequest<BrokerStatusResponse>;
