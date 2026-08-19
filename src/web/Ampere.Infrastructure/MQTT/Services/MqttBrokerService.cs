using System.Net;
using System.Text;
using Ampere.Application.Common.Abstractions;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace Ampere.Infrastructure.MQTT.Services;

/// <summary>Runs the local MQTT broker.</summary>
/// <param name="repository">The configuration repository.</param>
/// <param name="runtime">The shared broker runtime.</param>
public sealed class MqttBrokerService(
    IRepository<MqttBrokerConfigurationEntity> repository,
    MqttBrokerRuntime runtime) : IMqttBrokerService
{
    /// <inheritdoc />
    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        if (runtime.Server?.IsStarted == true)
        {
            return;
        }

        MqttBrokerConfigurationEntity? configuration =
            await repository.FirstOrDefaultAsync(
                _ => true,
                [],
                cancellationToken);

        if (configuration is null)
        {
            throw new InvalidOperationException(
                "MQTT broker is not configured.");
        }

        if (configuration.UseTls)
        {
            throw new NotSupportedException(
                "MQTT TLS requires a certificate.");
        }

        MqttServerFactory factory = new();
        MqttServerOptionsBuilder options =
            factory.CreateServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(
                    configuration.Port);

        if (!string.IsNullOrWhiteSpace(
            configuration.BindAddress))
        {
            IPAddress address = IPAddress.Parse(
                configuration.BindAddress);
            options.WithDefaultEndpointBoundIPAddress(
                address);
        }

        MqttServer server = factory.CreateMqttServer(
            options.Build());

        server.InterceptingPublishAsync +=
            OnMessageReceivedAsync;

        await server.StartAsync();
        runtime.Server = server;
        runtime.StartedAt = DateTimeOffset.UtcNow;
    }

    /// <inheritdoc />
    public async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (runtime.Server is null)
        {
            return;
        }

        await runtime.Server.StopAsync();
        runtime.Server.Dispose();
        runtime.Server = null;
        runtime.StartedAt = null;
    }

    /// <inheritdoc />
    public async Task RestartAsync(
        CancellationToken cancellationToken)
    {
        await StopAsync(cancellationToken);
        await StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BrokerStatusResponse> GetStatusAsync(
        CancellationToken cancellationToken)
    {
        MqttBrokerConfigurationEntity? configuration =
            await repository.FirstOrDefaultAsync(
                _ => true,
                [],
                cancellationToken);

        int clientCount = 0;

        if (runtime.Server?.IsStarted == true)
        {
            IList<MqttClientStatus> clients =
                await runtime.Server.GetClientsAsync();
            clientCount = clients.Count;
        }

        return new BrokerStatusResponse(
            runtime.Server?.IsStarted == true,
            runtime.StartedAt,
            configuration?.Port ?? 0,
            configuration?.BindAddress,
            clientCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MqttClientResponse>>
        GetClientsAsync(
            CancellationToken cancellationToken)
    {
        if (runtime.Server?.IsStarted != true)
        {
            return [];
        }

        IList<MqttClientStatus> clients =
            await runtime.Server.GetClientsAsync();

        return clients
            .Select(client => new MqttClientResponse(
                client.Id,
                null,
                client.ConnectedTimestamp,
                client.RemoteEndPoint?.ToString()))
            .ToArray();
    }

    /// <inheritdoc />
    public async Task PublishAsync(
        string topic,
        byte[] payload,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        if (runtime.Server?.IsStarted != true)
        {
            throw new InvalidOperationException(
                "MQTT broker is not running.");
        }

        MqttApplicationMessage message =
            new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(
                    MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

        InjectedMqttApplicationMessage injected =
            new(message);

        await runtime.Server.InjectApplicationMessage(
            injected,
            cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<MqttTopicMessageResponse>
        WatchMessagesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken)
    {
        await foreach (MqttTopicMessageResponse message
            in runtime.Messages.ReadAllAsync(cancellationToken))
        {
            yield return message;
        }
    }

    private Task OnMessageReceivedAsync(
        InterceptingPublishEventArgs args)
    {
        string payload = Encoding.UTF8.GetString(
            args.ApplicationMessage.Payload);

        runtime.Publish(
            new MqttTopicMessageResponse(
                args.ApplicationMessage.Topic,
                payload,
                args.ClientId,
                DateTimeOffset.UtcNow));

        return Task.CompletedTask;
    }
}
