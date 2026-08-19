using Ampere.Application.MQTT.Requests;
using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Publishes a message to a broker topic.</summary>
/// <param name="Request">The publish request.</param>
public sealed record PublishMessageCommand(PublishMessageRequest Request) : IRequest;
