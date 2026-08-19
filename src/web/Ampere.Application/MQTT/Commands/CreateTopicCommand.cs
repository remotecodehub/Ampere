using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Creates a configured MQTT topic.</summary>
/// <param name="Request">The topic request.</param>
public sealed record CreateTopicCommand(
    CreateTopicRequest Request)
    : ITransactionalRequest<Response<MqttTopicResponse>>;
