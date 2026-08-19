using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;
using Ampere.Infrastructure.MQTT.Services;
using Ampere.UnitTests.Common.Fixtures;
using Ampere.UnitTests.Common.Mocks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using Xunit;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests the MQTT broker service.</summary>
public sealed class MqttBrokerServiceTests
{
    [Fact]
    public async Task Start_WhenAlreadyRunning_DoesNothing()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
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
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartAsync(CancellationToken.None));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Start_WhenTlsEnabled_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                Port = MqttTestPort.Get(),
                UseTls = true
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => service.StartAsync(CancellationToken.None));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Start_InvalidBindAddress_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "not-an-ip",
                Port = MqttTestPort.Get()
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<FormatException>(
            () => service.StartAsync(CancellationToken.None));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Start_WithConfiguration_StartsServer()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = MqttTestPort.Get()
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await service.StartAsync(CancellationToken.None);

        Assert.True(runtime.Server!.IsStarted);
        Assert.NotNull(runtime.StartedAt);
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Stop_WhenNotRunning_DoesNothing()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await service.StopAsync(CancellationToken.None);

        Assert.Null(runtime.Server);
        Assert.Null(runtime.StartedAt);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Stop_WhenRunning_ClearsRuntime()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = MqttTestPort.Get()
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync(CancellationToken.None);

        await service.StopAsync(CancellationToken.None);

        Assert.Null(runtime.Server);
        Assert.Null(runtime.StartedAt);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Restart_WhenConfigured_RestartsServer()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = MqttTestPort.Get()
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await service.RestartAsync(CancellationToken.None);

        Assert.True(runtime.Server!.IsStarted);
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Restart_WhenNotConfigured_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RestartAsync(CancellationToken.None));

        Assert.Null(runtime.Server);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task GetStatus_WhenStopped_ReturnsConfiguration()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "0.0.0.0",
                Port = 1883
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        BrokerStatusResponse result = await service.GetStatusAsync(
            CancellationToken.None);

        Assert.False(result.IsRunning);
        Assert.Equal(1883, result.Port);
        Assert.Equal("0.0.0.0", result.BindAddress);
        Assert.Equal(0, result.ClientCount);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task GetStatus_WhenRunning_ReturnsClientCount()
    {
        int port = MqttTestPort.Get();
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = port
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync(CancellationToken.None);
        MqttClient client = await ConnectClientAsync(port);

        BrokerStatusResponse result = await service.GetStatusAsync(
            CancellationToken.None);

        Assert.True(result.IsRunning);
        Assert.Equal(1, result.ClientCount);
        await client.DisconnectAsync();
        client.Dispose();
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task GetClients_WhenStopped_ReturnsEmpty()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        IReadOnlyList<MqttClientResponse> clients =
            await service.GetClientsAsync(CancellationToken.None);

        Assert.Empty(clients);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task GetClients_WhenRunning_ReturnsClient()
    {
        int port = MqttTestPort.Get();
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = port
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync(CancellationToken.None);
        MqttClient client = await ConnectClientAsync(port);

        IReadOnlyList<MqttClientResponse> clients =
            await service.GetClientsAsync(CancellationToken.None);

        Assert.NotEmpty(clients);
        await client.DisconnectAsync();
        client.Dispose();
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Publish_WhenStopped_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync(
                "test/topic", [1, 2, 3], CancellationToken.None));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Publish_EmptyTopic_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.PublishAsync(" ", [], CancellationToken.None));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Publish_WhenRunning_PublishesToClient()
    {
        int port = MqttTestPort.Get();
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = port
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync(CancellationToken.None);
        MqttClient client = await ConnectClientAsync(port);
        TaskCompletionSource<string> received = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        client.ApplicationMessageReceivedAsync += args =>
        {
            string payload = args.ApplicationMessage.Payload.Length == 0
                ? string.Empty
                : System.Text.Encoding.UTF8.GetString(
                    args.ApplicationMessage.Payload);
            received.TrySetResult(payload);
            return Task.CompletedTask;
        };
        await client.SubscribeAsync("test/topic");

        await service.PublishAsync(
            "test/topic",
            System.Text.Encoding.UTF8.GetBytes("hello"),
            CancellationToken.None);

        string result = await received.Task.WaitAsync(
            TimeSpan.FromSeconds(5));
        Assert.Equal("hello", result);
        await client.DisconnectAsync();
        client.Dispose();
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task WatchMessages_WhenMessageArrives_YieldsMessage()
    {
        int port = MqttTestPort.Get();
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        await repository.AddAsync(
            new MqttBrokerConfigurationEntity
            {
                BindAddress = "127.0.0.1",
                Port = port
            }, CancellationToken.None);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync(CancellationToken.None);
        MqttClient client = await ConnectClientAsync(port);
        Task<MqttTopicMessageResponse> messageTask =
            ReadFirstMessageAsync(service);
        await client.PublishAsync(
            new MqttApplicationMessageBuilder()
                .WithTopic("telemetry/power")
                .WithPayload("42")
                .Build());

        MqttTopicMessageResponse message = await messageTask.WaitAsync(
            TimeSpan.FromSeconds(5));
        Assert.Equal("telemetry/power", message.Topic);
        Assert.Equal("42", message.Payload);
        Assert.Equal(client.Options.ClientId, message.ClientId);
        await client.DisconnectAsync();
        client.Dispose();
        await service.StopAsync(CancellationToken.None);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task WatchMessages_CancelledStreamStops()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        using CancellationTokenSource cancellation =
            new(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () =>
            {
                await foreach (MqttTopicMessageResponse _ in
                    service.WatchMessagesAsync(cancellation.Token))
                {
                }
            });

        await runtime.DisposeAsync();
    }

    private static async Task<MqttClient> ConnectClientAsync(int port)
    {
        MqttClientFactory factory = new();
        MqttClient client = factory.CreateMqttClient();
        MqttClientOptions options = factory
            .CreateClientOptionsBuilder()
            .WithTcpServer("127.0.0.1", port)
            .WithClientId(Guid.NewGuid().ToString())
            .Build();
        await client.ConnectAsync(options);
        return client;
    }

    private static async Task<MqttTopicMessageResponse>
        ReadFirstMessageAsync(MqttBrokerService service)
    {
        await foreach (MqttTopicMessageResponse message in
            service.WatchMessagesAsync(CancellationToken.None))
        {
            return message;
        }

        throw new InvalidOperationException(
            "The MQTT message stream ended unexpectedly.");
    }
}
