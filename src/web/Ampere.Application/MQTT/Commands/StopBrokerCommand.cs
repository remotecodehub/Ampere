using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Stops the MQTT broker.</summary>
public sealed record StopBrokerCommand
    : IRequest<BrokerStatusResponse>;
