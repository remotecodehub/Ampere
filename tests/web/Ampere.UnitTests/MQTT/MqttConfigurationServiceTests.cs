using Ampere.Infrastructure.MQTT.Models;
using Ampere.Infrastructure.MQTT.Services;
using Ampere.UnitTests.Common.Mocks;
using Xunit;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests persisted MQTT configuration rules.</summary>
public sealed class MqttConfigurationServiceTests
{
    [Fact]
    public async Task GetConfiguration_WhenMissing_ReturnsNull()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttConfigurationService service = new(repository);

        var result = await service.GetConfigurationAsync(
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetConfiguration_WhenPresent_ReturnsValues()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerConfigurationEntity entity = new()
        {
            BindAddress = "127.0.0.1",
            Port = 1883,
            StartOnBoot = true,
            UseTls = false
        };
        await repository.AddAsync(entity, CancellationToken.None);
        MqttConfigurationService service = new(repository);

        var result = await service.GetConfigurationAsync(
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.BindAddress, result.BindAddress);
        Assert.Equal(entity.Port, result.Port);
        Assert.Equal(entity.StartOnBoot, result.StartOnBoot);
        Assert.Equal(entity.UseTls, result.UseTls);
    }

    [Fact]
    public async Task SaveConfiguration_WhenMissing_AddsEntity()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttConfigurationService service = new(repository);
        ConfigureBrokerRequest request = new(
            "0.0.0.0", 1883, true, false);

        var result = await service.SaveConfigurationAsync(
            request, CancellationToken.None);

        Assert.Equal(1883, result.Port);
        Assert.Equal("0.0.0.0", result.BindAddress);
        Assert.True(result.StartOnBoot);
        Assert.False(result.UseTls);
        Assert.NotEmpty(repository.Items);
    }

    [Fact]
    public async Task SaveConfiguration_WhenPresent_UpdatesEntity()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerConfigurationEntity entity = new()
        {
            BindAddress = "127.0.0.1",
            Port = 1883,
            StartOnBoot = false,
            UseTls = false
        };
        await repository.AddAsync(entity, CancellationToken.None);
        MqttConfigurationService service = new(repository);
        ConfigureBrokerRequest request = new(
            "192.168.1.10", 1884, true, true);

        var result = await service.SaveConfigurationAsync(
            request, CancellationToken.None);

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("192.168.1.10", result.BindAddress);
        Assert.Equal(1884, result.Port);
        Assert.True(result.StartOnBoot);
        Assert.True(result.UseTls);
        Assert.Single(repository.Items);
    }
}
