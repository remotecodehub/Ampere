using Ampere.Domain.Common;
using Ampere.Domain.MQTT.Entities;
using Xunit;

namespace Ampere.UnitTests.Common;

/// <summary>Tests common and MQTT domain models.</summary>
public sealed class DomainModelTests
{
    [Fact]
    public void EntityBase_GeneratesOrKeepsIdentifier()
    {
        TestEntity generated = new();
        TestEntity explicitId = new("explicit");

        Assert.False(string.IsNullOrWhiteSpace(generated.Id));
        Assert.Equal("explicit", explicitId.Id);
        Assert.NotEqual(default, generated.CreatedAt);
        Assert.NotEqual(default, generated.UpdatedAt);
        Assert.True(generated.UpdatedAt >= generated.CreatedAt);
    }

    [Fact]
    public void BrokerConfiguration_StoresBrokerSettings()
    {
        BrokerConfiguration configuration = new()
        {
            BindAddress = "127.0.0.1",
            Port = 1883,
            StartOnBoot = true,
            UseTls = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        Assert.False(string.IsNullOrWhiteSpace(
            configuration.Id));
        Assert.Equal(1883, configuration.Port);
        Assert.True(configuration.StartOnBoot);
        Assert.False(configuration.UseTls);
    }

    private sealed class TestEntity(string id = "") : EntityBase(id);
}
