using Ampere.Application.Common.Abstractions;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Responses;
using Ampere.Domain.Common;
using Ampere.Infrastructure.MQTT.Models;
using Ampere.Infrastructure.MQTT.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests automatic MQTT broker startup.</summary>
public sealed class MqttBrokerHostedServiceTests
{
    [Fact]
    public async Task StartAsync_WhenDisabled_DoesNotStartBroker()
    {
        FakeRepository repository = new();
        repository.Entity.StartOnBoot = false;
        FakeBroker broker = new();
        await using ServiceProvider provider =
            CreateProvider(repository, broker);
        MqttBrokerHostedService hosted = new(
            provider.GetRequiredService<IServiceScopeFactory>());

        await hosted.StartAsync(CancellationToken.None);

        Assert.Equal(0, broker.StartCount);
    }

    [Fact]
    public async Task StartAsync_WhenEnabled_StartsBroker()
    {
        FakeRepository repository = new();
        repository.Entity.StartOnBoot = true;
        FakeBroker broker = new();
        await using ServiceProvider provider =
            CreateProvider(repository, broker);
        MqttBrokerHostedService hosted = new(
            provider.GetRequiredService<IServiceScopeFactory>());

        await hosted.StartAsync(CancellationToken.None);
        await hosted.StopAsync(CancellationToken.None);

        Assert.Equal(1, broker.StartCount);
    }

    private static ServiceProvider CreateProvider(
        FakeRepository repository,
        FakeBroker broker)
    {
        ServiceCollection services = new();
        services.AddScoped<
            IRepository<MqttBrokerConfigurationEntity>>(
            _ => repository);
        services.AddScoped<IMqttBrokerService>(
            _ => broker);
        return services.BuildServiceProvider();
    }

    private sealed class FakeRepository :
        IRepository<MqttBrokerConfigurationEntity>
    {
        public MqttBrokerConfigurationEntity Entity { get; } = new()
        {
            Id = Guid.CreateVersion7().ToString(),
            Port = 1883,
            BindAddress = "127.0.0.1"
        };

        public Task<MqttBrokerConfigurationEntity?>
            GetByIdAsync(
                string id,
                System.Linq.Expressions.Expression<
                    Func<MqttBrokerConfigurationEntity, object?>>[]
                    includes,
                CancellationToken cancellationToken) =>
            Task.FromResult<MqttBrokerConfigurationEntity?>(
                Entity);

        public Task<MqttBrokerConfigurationEntity?>
            GetTrackedByIdAsync(
                string id,
                System.Linq.Expressions.Expression<
                    Func<MqttBrokerConfigurationEntity, object?>>[]
                    includes,
                CancellationToken cancellationToken) =>
            Task.FromResult<MqttBrokerConfigurationEntity?>(
                Entity);

        public Task<MqttBrokerConfigurationEntity?>
            FirstOrDefaultAsync(
                System.Linq.Expressions.Expression<
                    Func<MqttBrokerConfigurationEntity, bool>>
                    predicate,
                System.Linq.Expressions.Expression<
                    Func<MqttBrokerConfigurationEntity, object?>>[]
                    includes,
                CancellationToken cancellationToken) =>
            Task.FromResult<MqttBrokerConfigurationEntity?>(
                Entity);

        public Task<IReadOnlyList<
            MqttBrokerConfigurationEntity>> ListAsync(
                System.Linq.Expressions.Expression<
                    Func<MqttBrokerConfigurationEntity, bool>>?
                    predicate,
                System.Linq.Expressions.Expression<
                    Func<MqttBrokerConfigurationEntity, object?>>[]
                    includes,
                CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<
                MqttBrokerConfigurationEntity>>([Entity]);

        public Task<bool> ExistsAsync(
            System.Linq.Expressions.Expression<
                Func<MqttBrokerConfigurationEntity, bool>> predicate,
            CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task AddAsync(
            MqttBrokerConfigurationEntity entity,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public void Update(
            MqttBrokerConfigurationEntity entity)
        {
        }

        public void Remove(
            MqttBrokerConfigurationEntity entity)
        {
        }

        public void RemoveRange(
            IEnumerable<MqttBrokerConfigurationEntity> entities)
        {
        }
    }

    private sealed class FakeBroker : IMqttBrokerService
    {
        public int StartCount { get; private set; }

        public Task StartAsync(
            CancellationToken cancellationToken)
        {
            StartCount++;
            return Task.CompletedTask;
        }

        public Task StopAsync(
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task RestartAsync(
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<BrokerStatusResponse> GetStatusAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(new BrokerStatusResponse(
                true, DateTimeOffset.UtcNow,
                1883, "127.0.0.1", 0));

        public Task<IReadOnlyList<MqttClientResponse>>
            GetClientsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MqttClientResponse>>([]);

        public Task PublishAsync(
            string topic, byte[] payload,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public async IAsyncEnumerable<
            MqttTopicMessageResponse> WatchMessagesAsync(
                [System.Runtime.CompilerServices.EnumeratorCancellation]
                CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
