using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Handles broker configuration changes.</summary>
/// <param name="configurationService">
/// The configuration service.
/// </param>
/// <param name="brokerService">The broker service.</param>
public sealed class ConfigureBrokerCommandHandler(
    IMqttConfigurationService configurationService,
    IMqttBrokerService brokerService)
    : IRequestHandler<ConfigureBrokerCommand,
        Response<BrokerConfigurationResponse>>
{
    /// <inheritdoc />
    public async ValueTask<Response<BrokerConfigurationResponse>>
        Handle(
            ConfigureBrokerCommand request,
            CancellationToken cancellationToken)
    {
        BrokerConfigurationResponse saved =
            await configurationService.SaveConfigurationAsync(
                request.Request,
                cancellationToken);

        await brokerService.RestartAsync(
            cancellationToken);

        return Response.Success(saved);
    }
}
