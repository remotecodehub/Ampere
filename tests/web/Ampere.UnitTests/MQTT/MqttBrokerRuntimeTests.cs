using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Services;
using Xunit;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests shared MQTT broker runtime state.</summary>
public sealed class MqttBrokerRuntimeTests
{
    [Fact]
    public async Task Publish_MakesMessageAvailable()
    {
        MqttBrokerRuntime runtime = new();
        MqttTopicMessageResponse message =
            new(
                "energy/main",
                "42",
                "device-1",
                DateTimeOffset.UtcNow);

        runtime.Publish(message);

        MqttTopicMessageResponse result =
            await runtime.Messages.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(message, result);
        await runtime.DisposeAsync();
    }
}
