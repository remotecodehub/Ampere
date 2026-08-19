using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;
using Ampere.Infrastructure.MQTT.Services;
using Ampere.UnitTests.Common.Fixtures;
using Ampere.UnitTests.Common.Mocks;
using Xunit;

namespace Ampere.UnitTests.MQTT;

/// <summary>Exercises MQTT broker behavior branches.</summary>
public sealed class MqttBrokerServiceBehaviorTests
{
    [Fact]
    public async Task GetClients_WhenRunning_ReturnsClients()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = MqttTestPort.Get()
            },
            CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.StartAsync(CancellationToken.None);

        IReadOnlyList<MqttClientResponse> clients =
            await service.GetClientsAsync(
                CancellationToken.None);

        Assert.Empty(clients);
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task GetStatus_WhenRunning_ReturnsRunningState()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = MqttTestPort.Get()
            },
            CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.StartAsync(CancellationToken.None);
        BrokerStatusResponse result =
            await service.GetStatusAsync(
                CancellationToken.None);

        Assert.True(result.IsRunning);
        Assert.NotNull(result.StartedAt);
        Assert.Equal(0, result.ConnectedClientsCount);
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Publish_WhenRunning_IsAccepted()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = MqttTestPort.Get()
            },
            CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.StartAsync(CancellationToken.None);

        await service.PublishAsync(
            "energy/device-1",
            [1, 2, 3],
            CancellationToken.None);

        Assert.True(runtime.Server!.IsStarted);
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task WatchMessages_Cancellation_StopsEnumeration()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);
        using CancellationTokenSource source = new();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
            {
                await foreach (MqttTopicMessageResponse _
                    in service.WatchMessagesAsync(
                        source.Token))
                {
                }
            });

        await runtime.DisposeAsync();
    }
}
