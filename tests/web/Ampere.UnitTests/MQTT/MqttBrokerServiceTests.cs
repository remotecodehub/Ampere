using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;
using Ampere.Infrastructure.MQTT.Services;
using Ampere.UnitTests.Common.Mocks;
using MQTTnet;
using MQTTnet.Server;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests the MQTT broker service.</summary>
public sealed class MqttBrokerServiceTests
{
    [Fact]
    public async Task Start_WhenAlreadyRunning_DoesNothing()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);
        MqttServerFactory factory = new();
        MqttServer server = factory.CreateMqttServer(
            factory.CreateServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(0)
                .Build());
        runtime.Server = server;

        await service.StartAsync(CancellationToken.None);

        Assert.Same(server, runtime.Server);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Start_WhenNotConfigured_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartAsync(
                CancellationToken.None));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Start_WhenTlsEnabled_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                Port = 0,
                UseTls = true
            },
            CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => service.StartAsync(
                CancellationToken.None));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Start_WithConfiguration_StartsServer()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = 0
            },
            CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.StartAsync(CancellationToken.None);

        MqttServer server = runtime.Server!;
        Assert.True(server.IsStarted);
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Stop_WhenNotRunning_DoesNothing()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.StopAsync(CancellationToken.None);

        Assert.Null(runtime.Server);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Restart_WhenConfigured_RestartsServer()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = 0
            },
            CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await service.RestartAsync(
            CancellationToken.None);

        MqttServer server = runtime.Server!;
        Assert.True(server.IsStarted);
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Publish_WhenStopped_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync(
                "test/topic",
                [1, 2, 3],
                CancellationToken.None));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task GetStatus_WhenStopped_ReturnsConfiguration()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "0.0.0.0",
                Port = 1883
            },
            CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service =
            new(repository, runtime);

        BrokerStatusResponse result =
            await service.GetStatusAsync(
                CancellationToken.None);

        Assert.False(result.Running);
        Assert.Equal(1883, result.Port);
        Assert.Equal("0.0.0.0", result.BindAddress);
        await runtime.DisposeAsync();
    }
}
