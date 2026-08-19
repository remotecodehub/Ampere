using Ampere.Application.MQTT.Requests;
using Ampere.Infrastructure.MQTT.Models;
using Ampere.Infrastructure.MQTT.Services;
using Ampere.UnitTests.Common.Mocks;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests MQTT configuration persistence.</summary>
public sealed class MqttConfigurationServiceTests
{
    [Fact]
    public async Task GetConfiguration_WhenEmpty_ReturnsNull()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        MqttConfigurationService service =
            new(repository);

        object? result = await service
            .GetConfigurationAsync(
                CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveConfiguration_WhenEmpty_AddsEntity()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        MqttConfigurationService service =
            new(repository);
        ConfigureBrokerRequest request = new(
            "127.0.0.1",
            1883,
            true,
            false);

        var result = await service.SaveConfigurationAsync(
            request,
            CancellationToken.None);

        Assert.Equal("127.0.0.1", result.BindAddress);
        Assert.Equal(1883, result.Port);
        Assert.Single(repository.Entities);
    }

    [Fact]
    public async Task SaveConfiguration_WhenExists_UpdatesEntity()
    {
        FakeRepository<MqttBrokerConfigurationEntity>
            repository = new();
        MqttBrokerConfigurationEntity existing = new()
        {
            BindAddress = "0.0.0.0",
            Port = 1883
        };
        await repository.AddAsync(
            existing,
            CancellationToken.None);
        MqttConfigurationService service =
            new(repository);

        var result = await service.SaveConfigurationAsync(
            new ConfigureBrokerRequest(
                "127.0.0.1",
                1884,
                false,
                true),
            CancellationToken.None);

        Assert.Equal("127.0.0.1", result.BindAddress);
        Assert.Equal(1884, result.Port);
        Assert.False(result.StartOnBoot);
        Assert.True(result.UseTls);
        Assert.Single(repository.Entities);
    }
}
