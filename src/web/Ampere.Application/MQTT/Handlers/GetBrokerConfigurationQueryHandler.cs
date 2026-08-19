using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Responses;
using Mediator;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Gets persisted broker configuration.</summary>
/// <param name="configurationService">
/// The configuration service.
/// </param>
public sealed class GetBrokerConfigurationQueryHandler(
    IMqttConfigurationService configurationService)
    : IRequestHandler<GetBrokerConfigurationQuery,
        Response<BrokerConfigurationResponse?>>
{
    /// <inheritdoc />
    public async ValueTask<Response<BrokerConfigurationResponse?>>
        Handle(
            GetBrokerConfigurationQuery request,
            CancellationToken cancellationToken)
    {
        BrokerConfigurationResponse? configuration =
            await configurationService.GetConfigurationAsync(
                cancellationToken);
        return Response.Success(configuration);
    }
}
