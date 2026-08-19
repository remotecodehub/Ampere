using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Responses;
using Mediator.Net.Context;
using Mediator.Net.Contracts;

namespace Ampere.Application.MQTT.Handlers;

/// <summary>Handles retrieval of the persisted broker configuration.</summary>
public sealed class GetBrokerConfigurationQueryHandler(IMqttConfigurationService configService) : IRequestHandler<GetBrokerConfigurationQuery, Ampere.Application.Common.Responses.Response<Ampere.Application.MQTT.Responses.BrokerConfigurationResponse?>>
{
    public async Task<Ampere.Application.Common.Responses.Response<Ampere.Application.MQTT.Responses.BrokerConfigurationResponse?>> Handle(IReceiveContext<GetBrokerConfigurationQuery> context, CancellationToken cancellationToken)
    {
        var cfg = await configService.GetConfigurationAsync(cancellationToken);
        return Ampere.Application.Common.Responses.Response.Success(cfg);
    }
}
