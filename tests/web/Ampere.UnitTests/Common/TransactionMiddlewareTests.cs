using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.Persistence.Middlewares;
using Mediator;
using Xunit;

namespace Ampere.UnitTests.Common;

/// <summary>Tests transactional Mediator pipeline behavior.</summary>
public sealed class TransactionMiddlewareTests
{
    [Fact]
    public async Task NonTransactionalMessage_BypassesUnitOfWork()
    {
        FakeUnitOfWork unitOfWork = new();
        TransactionMiddleware<StartBrokerCommand,
            BrokerStatusResponse> middleware = new(unitOfWork);
        MessageHandlerDelegate<StartBrokerCommand,
            BrokerStatusResponse> next = NextStatus;

        BrokerStatusResponse result = await middleware.Handle(
            new StartBrokerCommand(), next,
            CancellationToken.None);

        Assert.Equal(1883, result.Port);
        Assert.Equal(0, unitOfWork.BeginCount);
    }

    [Fact]
    public async Task TransactionalMessage_CommitsAndSaves()
    {
        FakeUnitOfWork unitOfWork = new();
        TransactionMiddleware<ConfigureBrokerCommand,
            Response<BrokerConfigurationResponse>> middleware =
            new(unitOfWork);
        MessageHandlerDelegate<ConfigureBrokerCommand,
            Response<BrokerConfigurationResponse>> next =
            NextConfiguration;

        Response<BrokerConfigurationResponse> result =
            await middleware.Handle(
                new ConfigureBrokerCommand(
                    new ConfigureBrokerRequest(
                        "127.0.0.1", 1883, true, false)),
                next,
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, unitOfWork.BeginCount);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(1, unitOfWork.CommitCount);
        Assert.Equal(0, unitOfWork.RollbackCount);
    }

    [Fact]
    public async Task TransactionalMessage_RollsBackWhenNextFails()
    {
        FakeUnitOfWork unitOfWork = new();
        TransactionMiddleware<ConfigureBrokerCommand,
            Response<BrokerConfigurationResponse>> middleware =
            new(unitOfWork);
        MessageHandlerDelegate<ConfigureBrokerCommand,
            Response<BrokerConfigurationResponse>> next =
            ThrowingNext;

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await middleware.Handle(
                new ConfigureBrokerCommand(
                    new ConfigureBrokerRequest(
                        "127.0.0.1", 1883, true, false)),
                next,
                CancellationToken.None));

        Assert.Equal(1, unitOfWork.BeginCount);
        Assert.Equal(0, unitOfWork.SaveCount);
        Assert.Equal(0, unitOfWork.CommitCount);
        Assert.Equal(1, unitOfWork.RollbackCount);
    }

    private static ValueTask<BrokerStatusResponse> NextStatus(
        StartBrokerCommand message,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(
            new BrokerStatusResponse(
                false, null, 1883,
                "127.0.0.1", 0));
    }

    private static ValueTask<Response<
        BrokerConfigurationResponse>> NextConfiguration(
        ConfigureBrokerCommand message,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return ValueTask.FromResult(
            Response.Success(
                new BrokerConfigurationResponse(
                    "id", message.Request.BindAddress,
                    message.Request.Port,
                    message.Request.StartOnBoot,
                    message.Request.UseTls, now, now)));
    }

    private static ValueTask<Response<
        BrokerConfigurationResponse>> ThrowingNext(
        ConfigureBrokerCommand message,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("handler failure");
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int BeginCount { get; private set; }
        public int SaveCount { get; private set; }
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(
            CancellationToken cancellationToken)
        {
            BeginCount++;
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(
            CancellationToken cancellationToken)
        {
            CommitCount++;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(
            CancellationToken cancellationToken)
        {
            RollbackCount++;
            return Task.CompletedTask;
        }
    }
}
