using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Requests;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Publishes a message to a broker topic.</summary>
/// <param name="Request">The publish request.</param>
public sealed record PublishMessageCommand(
    PublishMessageRequest Request)
    : ITransactionalRequest<Response<bool>>;
