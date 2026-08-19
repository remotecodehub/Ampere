using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Responses;
using Ampere.Application.MQTT.Requests;
using Mediator.Net.Context;
using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Handles broker configuration persistence and
/// applies it to the runtime broker service.</summary>
public sealed class ConfigureBrokerCommandHandler(
    IMqttConfigurationService configurationService,
    IMqttBrokerService brokerService) : IRequestHandler<ConfigureBrokerCommand, Ampere.Application.Common.Responses.Response<Ampere.Application.MQTT.Responses.BrokerConfigurationResponse>>
{
    /// <inheritdoc/>
    public async Task<Ampere.Application.Common.Responses.Response<Ampere.Application.MQTT.Responses.BrokerConfigurationResponse>> Handle(IReceiveContext<ConfigureBrokerCommand> context, CancellationToken cancellationToken)
    {
        ConfigureBrokerRequest req = context.Message.Request;
        BrokerConfigurationResponse saved = await configurationService.SaveConfigurationAsync(req, cancellationToken);

        // If configuration requests the broker to run, restart to apply
        // configuration change.
        await brokerService.RestartAsync(cancellationToken);

        return Ampere.Application.Common.Responses.Response.Success(saved);
    }
}
