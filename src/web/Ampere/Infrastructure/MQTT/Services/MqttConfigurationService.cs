using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;
using Microsoft.EntityFrameworkCore;

namespace Ampere.Infrastructure.MQTT.Services;

/// <summary>
/// Persists and retrieves MQTT broker configuration using
/// the application database.
/// </summary>
public sealed class MqttConfigurationService(Persistence.AmpereDbContext db) : IMqttConfigurationService
{
    private readonly Persistence.AmpereDbContext _db = db;

    public async Task<BrokerConfigurationResponse?> GetConfigurationAsync(CancellationToken cancellationToken)
    {
        var entity = await _db.MqttBrokerConfigurations.OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(cancellationToken);
        if (entity is null) return null;
        return new BrokerConfigurationResponse(entity.Id, entity.BindAddress, entity.Port, entity.StartOnBoot, entity.UseTls, entity.CreatedAt, entity.UpdatedAt);
    }

    public async Task<BrokerConfigurationResponse> SaveConfigurationAsync(ConfigureBrokerRequest request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var existing = await _db.MqttBrokerConfigurations.OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            var entity = new MqttBrokerConfigurationEntity
            {
                Id = Guid.NewGuid(),
                BindAddress = request.BindAddress,
                Port = request.Port,
                StartOnBoot = request.StartOnBoot,
                UseTls = request.UseTls,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.MqttBrokerConfigurations.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return new BrokerConfigurationResponse(entity.Id, entity.BindAddress, entity.Port, entity.StartOnBoot, entity.UseTls, entity.CreatedAt, entity.UpdatedAt);
        }
        else
        {
            existing.BindAddress = request.BindAddress;
            existing.Port = request.Port;
            existing.StartOnBoot = request.StartOnBoot;
            existing.UseTls = request.UseTls;
            existing.UpdatedAt = now;

            _db.MqttBrokerConfigurations.Update(existing);
            await _db.SaveChangesAsync(cancellationToken);

            return new BrokerConfigurationResponse(existing.Id, existing.BindAddress, existing.Port, existing.StartOnBoot, existing.UseTls, existing.CreatedAt, existing.UpdatedAt);
        }
    }
}
