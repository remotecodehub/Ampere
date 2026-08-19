using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Abstractions;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Handlers;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Ampere.Domain.MQTT.Entities;
using Ampere.UnitTests.Common.Mocks;
using Xunit;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests MQTT application handlers.</summary>
public sealed class MqttHandlerTests
{
    [Fact]
    public async Task ConfigureBroker_SavesAndRestarts()
    {
        FakeConfiguration configuration = new();
        FakeBroker broker = new();
        ConfigureBrokerCommandHandler handler = new(
            configuration, broker);

        Response<BrokerConfigurationResponse> result =
            await handler.Handle(
                new ConfigureBrokerCommand(
                    new ConfigureBrokerRequest(
                        "127.0.0.1", 1883, true, false)),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, configuration.SaveCount);
        Assert.Equal(1, broker.RestartCount);
    }

    [Fact]
    public async Task ConfigurationQuery_ReturnsStoredValue()
    {
        FakeConfiguration configuration = new();
        GetBrokerConfigurationQueryHandler handler =
            new(configuration);

        Response<BrokerConfigurationResponse?> result =
            await handler.Handle(
                new GetBrokerConfigurationQuery(),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task BrokerStatusQuery_ReturnsBrokerStatus()
    {
        FakeBroker broker = new();
        GetBrokerStatusQueryHandler handler = new(broker);

        BrokerStatusResponse result = await handler.Handle(
            new GetBrokerStatusQuery(), CancellationToken.None);

        Assert.Equal(1883, result.Port);
    }

    [Fact]
    public async Task ConnectedClientsQuery_ReturnsClients()
    {
        FakeBroker broker = new();
        GetConnectedClientsQueryHandler handler = new(broker);

        IReadOnlyList<MqttClientResponse> result =
            await handler.Handle(
                new GetConnectedClientsQuery(),
                CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("client-1", result[0].ClientId);
    }

    [Fact]
    public async Task TopicsQuery_MapsTopics()
    {
        FakeRepository<MqttTopic> repository = new();
        await repository.AddAsync(new MqttTopic
        {
            Name = "energy/main",
            Description = "Energy",
            Enabled = true
        }, CancellationToken.None);
        GetTopicsQueryHandler handler = new(repository);

        IReadOnlyList<MqttTopicResponse> result =
            await handler.Handle(
                new GetTopicsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("energy/main", result[0].Name);
    }

    [Fact]
    public async Task PublishHandler_ReturnsSuccessAndFailure()
    {
        FakeBroker broker = new();
        PublishMessageCommandHandler handler = new(broker);
        PublishMessageRequest request = new(
            "energy/main", [1, 2, 3]);

        Response<bool> success = await handler.Handle(
            new PublishMessageCommand(request),
            CancellationToken.None);
        broker.FailPublish = true;
        Response<bool> failure = await handler.Handle(
            new PublishMessageCommand(request),
            CancellationToken.None);

        Assert.True(success.Succeeded);
        Assert.False(failure.Succeeded);
    }

    [Fact]
    public async Task StartAndStopHandlers_DelegateToBroker()
    {
        FakeBroker broker = new();
        StartBrokerCommandHandler start = new(broker);
        StopBrokerCommandHandler stop = new(broker);

        await start.Handle(
            new StartBrokerCommand(), CancellationToken.None);
        await stop.Handle(
            new StopBrokerCommand(), CancellationToken.None);

        Assert.Equal(1, broker.StartCount);
        Assert.Equal(1, broker.StopCount);
    }

    [Fact]
    public async Task WatchTopicsHandler_StreamsMessages()
    {
        FakeBroker broker = new();
        WatchTopicsQueryHandler handler = new(broker);

        List<MqttTopicMessageResponse> messages = [];
        await foreach (MqttTopicMessageResponse message in
            handler.Handle(
                new WatchTopicsQuery(),
                CancellationToken.None))
        {
            messages.Add(message);
        }

        Assert.Single(messages);
        Assert.Equal("energy/main", messages[0].Topic);
    }

    private sealed class FakeConfiguration :
        IMqttConfigurationService
    {
        public int SaveCount { get; private set; }

        public Task<BrokerConfigurationResponse?>
            GetConfigurationAsync(
                CancellationToken cancellationToken)
        {
            return Task.FromResult<
                BrokerConfigurationResponse?>(
                    new BrokerConfigurationResponse(
                        "id", "127.0.0.1", 1883,
                        true, false,
                        DateTimeOffset.UtcNow,
                        DateTimeOffset.UtcNow));
        }

        public Task<BrokerConfigurationResponse>
            SaveConfigurationAsync(
                ConfigureBrokerRequest request,
                CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.FromResult(
                new BrokerConfigurationResponse(
                    "id", request.BindAddress, request.Port,
                    request.StartOnBoot, request.UseTls,
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow));
        }
    }

    private sealed class FakeBroker : IMqttBrokerService
    {
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }
        public int RestartCount { get; private set; }
        public bool FailPublish { get; set; }

        public Task StartAsync(
            CancellationToken cancellationToken)
        {
            StartCount++;
            return Task.CompletedTask;
        }

        public Task StopAsync(
            CancellationToken cancellationToken)
        {
            StopCount++;
            return Task.CompletedTask;
        }

        public Task RestartAsync(
            CancellationToken cancellationToken)
        {
            RestartCount++;
            return Task.CompletedTask;
        }

        public Task<BrokerStatusResponse> GetStatusAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new BrokerStatusResponse(
                    true, DateTimeOffset.UtcNow,
                    1883, "127.0.0.1", 1));
        }

        public Task<IReadOnlyList<MqttClientResponse>>
            GetClientsAsync(
                CancellationToken cancellationToken)
        {
            IReadOnlyList<MqttClientResponse> clients =
                [new("client-1", null, null, null)];
            return Task.FromResult(clients);
        }

        public Task PublishAsync(
            string topic,
            byte[] payload,
            CancellationToken cancellationToken)
        {
            if (FailPublish)
            {
                throw new InvalidOperationException(
                    "Broker is not running.");
            }

            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<
            MqttTopicMessageResponse> WatchMessagesAsync(
                [System.Runtime.CompilerServices.EnumeratorCancellation]
                CancellationToken cancellationToken)
        {
            await Task.Yield();
            yield return new MqttTopicMessageResponse(
                "energy/main", "42", "client-1",
                DateTimeOffset.UtcNow);
        }
    }
}
