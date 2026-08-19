using Ampere.Application.Common.Abstractions;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;

namespace Ampere.Infrastructure.MQTT.Services;

/// <summary>Persists MQTT broker configuration.</summary>
/// <param name="repository">The configuration repository.</param>
public sealed class MqttConfigurationService(
    IRepository<MqttBrokerConfigurationEntity> repository)
    : IMqttConfigurationService
{
    /// <inheritdoc />
    public async Task<BrokerConfigurationResponse?>
        GetConfigurationAsync(
            CancellationToken cancellationToken)
    {
        MqttBrokerConfigurationEntity? entity =
            await repository.FirstOrDefaultAsync(
                _ => true,
                [],
                cancellationToken);

        return entity is null
            ? null
            : ToResponse(entity);
    }

    /// <inheritdoc />
    public async Task<BrokerConfigurationResponse>
        SaveConfigurationAsync(
            ConfigureBrokerRequest request,
            CancellationToken cancellationToken)
    {
        MqttBrokerConfigurationEntity? existing =
            await repository.FirstOrDefaultAsync(
                _ => true,
                [],
                cancellationToken);

        if (existing is null)
        {
            MqttBrokerConfigurationEntity entity =
                new()
                {
                    BindAddress = request.BindAddress,
                    Port = request.Port,
                    StartOnBoot = request.StartOnBoot,
                    UseTls = request.UseTls
                };

            await repository.AddAsync(
                entity,
                cancellationToken);

            return ToResponse(entity);
        }

        existing.BindAddress = request.BindAddress;
        existing.Port = request.Port;
        existing.StartOnBoot = request.StartOnBoot;
        existing.UseTls = request.UseTls;
        repository.Update(existing);

        return ToResponse(existing);
    }

    private static BrokerConfigurationResponse ToResponse(
        MqttBrokerConfigurationEntity entity)
    {
        return new BrokerConfigurationResponse(
            entity.Id,
            entity.BindAddress,
            entity.Port,
            entity.StartOnBoot,
            entity.UseTls,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
