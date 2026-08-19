using Ampere.Application.Common.Abstractions;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Infrastructure.MQTT.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ampere.Infrastructure.MQTT.Services;

/// <summary>Starts the broker when configured for startup.</summary>
/// <param name="scopeFactory">The service scope factory.</param>
public sealed class MqttBrokerHostedService(
    IServiceScopeFactory scopeFactory)
    : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope =
            scopeFactory.CreateAsyncScope();

        IRepository<MqttBrokerConfigurationEntity>
            repository = scope.ServiceProvider
                .GetRequiredService<
                    IRepository<MqttBrokerConfigurationEntity>>();

        MqttBrokerConfigurationEntity? configuration =
            await repository.FirstOrDefaultAsync(
                _ => true,
                [],
                cancellationToken);

        if (configuration?.StartOnBoot != true)
        {
            return;
        }

        IMqttBrokerService service =
            scope.ServiceProvider.GetRequiredService<
                IMqttBrokerService>();
        await service.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
