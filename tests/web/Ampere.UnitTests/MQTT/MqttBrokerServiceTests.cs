using System.Text;
using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;
using Ampere.Infrastructure.MQTT.Services;
using Ampere.UnitTests.Common.Fixtures;
using Ampere.UnitTests.Common.Mocks;
using MQTTnet;
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

        await service.StartAsync( TestContext.Current.CancellationToken);

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
            () => service.StartAsync( TestContext.Current.CancellationToken));

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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => service.StartAsync( TestContext.Current.CancellationToken));

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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<FormatException>(
            () => service.StartAsync( TestContext.Current.CancellationToken));

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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await service.StartAsync( TestContext.Current.CancellationToken);

        Assert.True(runtime.Server!.IsStarted);
        Assert.NotNull(runtime.StartedAt);
        await service.StopAsync( TestContext.Current.CancellationToken);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Stop_WhenNotRunning_DoesNothing()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await service.StopAsync( TestContext.Current.CancellationToken);

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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync( TestContext.Current.CancellationToken);

        await service.StopAsync( TestContext.Current.CancellationToken);

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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await service.RestartAsync( TestContext.Current.CancellationToken);

        Assert.True(runtime.Server!.IsStarted);
        await service.StopAsync( TestContext.Current.CancellationToken);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Restart_WhenNotConfigured_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RestartAsync( TestContext.Current.CancellationToken));

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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        BrokerStatusResponse result = await service.GetStatusAsync(
             TestContext.Current.CancellationToken);

        Assert.False(result.IsRunning);
        Assert.Equal(1883, result.Port);
        Assert.Equal("0.0.0.0", result.BindAddress);
        Assert.Equal(0, result.ConnectedClientsCount);
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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync( TestContext.Current.CancellationToken);
        MqttClient client = await ConnectClientAsync(port);

        BrokerStatusResponse result = await service.GetStatusAsync(
             TestContext.Current.CancellationToken);

        Assert.True(result.IsRunning);
        Assert.Equal(1, result.ConnectedClientsCount);
        await client.DisconnectAsync(new(), TestContext.Current.CancellationToken);
        client.Dispose();
        await service.StopAsync( TestContext.Current.CancellationToken);
        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task GetClients_WhenStopped_ReturnsEmpty()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        IReadOnlyList<MqttClientResponse> clients =
            await service.GetClientsAsync( TestContext.Current.CancellationToken);

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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync( TestContext.Current.CancellationToken);
        MqttClient client = await ConnectClientAsync(port);

        IReadOnlyList<MqttClientResponse> clients =
            await service.GetClientsAsync( TestContext.Current.CancellationToken);

        Assert.NotEmpty(clients);
        await client.DisconnectAsync(new(), TestContext.Current.CancellationToken);
        client.Dispose();
        await service.StopAsync( TestContext.Current.CancellationToken);
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
                "test/topic", [1, 2, 3],  TestContext.Current.CancellationToken));

        await runtime.DisposeAsync();
    }

    [Fact]
    public async Task Publish_EmptyTopic_Throws()
    {
        FakeRepository<MqttBrokerConfigurationEntity> repository = new();
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.PublishAsync(" ", [],  TestContext.Current.CancellationToken));

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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync( TestContext.Current.CancellationToken);
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
        await client.SubscribeAsync("test/topic", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, TestContext.Current.CancellationToken);

        await service.PublishAsync(
            "test/topic",
            System.Text.Encoding.UTF8.GetBytes("hello"),
             TestContext.Current.CancellationToken);

        string result = await received.Task.WaitAsync(
            TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("hello", result);
        await client.DisconnectAsync(new(), TestContext.Current.CancellationToken);
        client.Dispose();
        await service.StopAsync(TestContext.Current.CancellationToken);
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
            },  TestContext.Current.CancellationToken);
        MqttBrokerRuntime runtime = new();
        MqttBrokerService service = new(repository, runtime);
        await service.StartAsync( TestContext.Current.CancellationToken);
        MqttClient client = await ConnectClientAsync(port);
        Task<MqttTopicMessageResponse> messageTask =
            ReadFirstMessageAsync(service);
        await client.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic("telemetry/power")
                .WithPayload("42")
                .Build(), TestContext.Current.CancellationToken);

        MqttTopicMessageResponse message = await messageTask.WaitAsync(
            TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("telemetry/power", message.Topic);
        Assert.Equal("42", message.Payload);
        Assert.Equal(client.Options.ClientId, message.ClientId);
        await client.DisconnectAsync(new(), TestContext.Current.CancellationToken);
        client.Dispose();
        await service.StopAsync( TestContext.Current.CancellationToken);
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
        MqttClient client = (MqttClient)factory.CreateMqttClient();
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
            service.WatchMessagesAsync( TestContext.Current.CancellationToken))
        {
            return message;
        }

        throw new InvalidOperationException(
            "The MQTT message stream ended unexpectedly.");
    }
}
