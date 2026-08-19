using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;
using Ampere.Infrastructure.MQTT.Services;
using Ampere.UnitTests.Common.Mocks;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests running MQTT broker operations.</summary>
public sealed class MqttBrokerServiceRuntimeTests
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
                Port = 0
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
    public async Task Publish_WhenRunning_Completes()
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
        await service.PublishAsync(
            "energy/main",
            [1, 2, 3],
            CancellationToken.None);

        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }
}
