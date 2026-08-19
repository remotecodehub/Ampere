using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Handlers;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Ampere.Domain.MQTT.Entities;
using Ampere.UnitTests.Common.Mocks;

namespace Ampere.UnitTests.MQTT;

/// <summary>Tests MQTT topic creation.</summary>
public sealed class CreateTopicCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTopicIsNew_CreatesTopic()
    {
        FakeRepository<MqttTopic> repository = new();
        CreateTopicCommandHandler handler =
            new(repository);

        Response<MqttTopicResponse> result =
            await handler.Handle(
                new CreateTopicCommand(
                    new CreateTopicRequest(
                        "energy/main",
                        "Main energy topic")),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(repository.Entities);
    }

    [Fact]
    public async Task Handle_WhenTopicExists_ReturnsFailure()
    {
        FakeRepository<MqttTopic> repository = new();
        await repository.AddAsync(
            new MqttTopic
            {
                Name = "energy/main"
            },
            CancellationToken.None);
        CreateTopicCommandHandler handler =
            new(repository);

        Response<MqttTopicResponse> result =
            await handler.Handle(
                new CreateTopicCommand(
                    new CreateTopicRequest(
                        "energy/main",
                        null)),
                CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Single(result.Errors);
        Assert.Single(repository.Entities);
    }
}
