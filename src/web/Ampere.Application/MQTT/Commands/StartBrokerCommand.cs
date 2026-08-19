using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Command to start the broker.</summary>
public sealed record StartBrokerCommand() : IRequest;
