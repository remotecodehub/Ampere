using Ampere.Application.MQTT.Responses;
using Xunit;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests MQTT response payloads.</summary>
public sealed class MqttResponseTests
{
    [Fact]
    public void Responses_StoreProvidedValues()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        BrokerConfigurationResponse configuration =
            new("id", "127.0.0.1", 1883,
                true, false, now, now);
        BrokerStatusResponse status = new(
            true, now, 1883, "127.0.0.1", 2);
        MqttClientResponse client = new(
            "client", "user", now, "127.0.0.1");
        MqttTopicResponse topic = new(
            "id", "energy/main", "Energy", true);
        MqttTopicMessageResponse message = new(
            "energy/main", "42", "client", now);

        Assert.Equal(1883, configuration.Port);
        Assert.True(status.IsRunning);
        Assert.Equal("client", client.ClientId);
        Assert.Equal("energy/main", topic.Name);
        Assert.Equal("42", message.Payload);
    }
}
