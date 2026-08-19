using System.Threading.Channels;
using Ampere.Application.MQTT.Responses;
using MQTTnet.Server;

namespace Ampere.Infrastructure.MQTT.Services;

/// <summary>Owns the process-wide MQTT broker runtime.</summary>
public sealed class MqttBrokerRuntime : IAsyncDisposable
{
    private readonly Channel<MqttTopicMessageResponse> _messages =
        Channel.CreateUnbounded<MqttTopicMessageResponse>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

    /// <summary>Gets the active MQTT server.</summary>
    public MqttServer? Server { get; set; }

    /// <summary>Gets the broker start timestamp.</summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>Gets the live message channel.</summary>
    public ChannelReader<MqttTopicMessageResponse>
        Messages => _messages.Reader;

    /// <summary>Publishes a message to live subscribers.</summary>
    /// <param name="message">The message.</param>
    public void Publish(MqttTopicMessageResponse message)
    {
        _messages.Writer.TryWrite(message);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _messages.Writer.TryComplete();

        if (Server is not null)
        {
            await Server.StopAsync();
            Server.Dispose();
            Server = null;
        }
    }
}
