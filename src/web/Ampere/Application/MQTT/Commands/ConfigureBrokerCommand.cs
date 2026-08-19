using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Command to persist and apply a broker configuration.</summary>
/// <param name="Request">The configuration request.</param>
public sealed record ConfigureBrokerCommand(ConfigureBrokerRequest Request) : IRequest;
