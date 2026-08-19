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
        await repository.AddAsync(new MqttBrokerConfigurationEntity
        {
            BindAddress = "127.0.0.1",
            Port = MqttTestPort.Get()
        },
        TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.StartAsync(TestContext.Current.CancellationToken);

        IReadOnlyList<MqttClientResponse> clients =
            await service.GetClientsAsync(
                TestContext.Current.CancellationToken);

        Assert.Empty(clients);
        await service.StopAsync(TestContext.Current.CancellationToken);
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
            TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.StartAsync(TestContext.Current.CancellationToken);
        BrokerStatusResponse result =
            await service.GetStatusAsync(
                TestContext.Current.CancellationToken);

        Assert.True(result.IsRunning);
        Assert.NotNull(result.StartedAt);
        Assert.Equal(0, result.ConnectedClientsCount);
        await service.StopAsync(TestContext.Current.CancellationToken);
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
            TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.StartAsync(TestContext.Current.CancellationToken);

        await service.PublishAsync(
            "energy/device-1",
            [1, 2, 3],
            TestContext.Current.CancellationToken);

        Assert.True(runtime.Server!.IsStarted);
        await service.StopAsync(TestContext.Current.CancellationToken);
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
