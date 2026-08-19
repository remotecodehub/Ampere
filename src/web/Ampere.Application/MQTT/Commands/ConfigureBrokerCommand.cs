using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;

namespace Ampere.Application.MQTT.Commands;

/// <summary>Persists and applies broker configuration.</summary>
/// <param name="Request">The broker configuration.</param>
public sealed record ConfigureBrokerCommand(
    ConfigureBrokerRequest Request)
    : ITransactionalRequest<
        Response<BrokerConfigurationResponse>>;
