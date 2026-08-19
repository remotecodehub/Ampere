using Ampere.Application.Common.Abstractions;
using Ampere.Application.MQTT.Responses;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Starts the MQTT broker.</summary>
public sealed record StartBrokerCommand
    : ITransactionalRequest<BrokerStatusResponse>;
