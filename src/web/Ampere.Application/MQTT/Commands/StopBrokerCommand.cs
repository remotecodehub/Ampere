using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Command to stop the broker.</summary>
public sealed record StopBrokerCommand() : IRequest;
