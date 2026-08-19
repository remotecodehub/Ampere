using Ampere.Application.Common.Abstractions;
using Ampere.Application.MQTT.Responses;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Stops the MQTT broker.</summary>
public sealed record StopBrokerCommand
    : ITransactionalRequest<BrokerStatusResponse>;
