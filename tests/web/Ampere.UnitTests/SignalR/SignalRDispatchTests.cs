using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Pipeline.Validation;
using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Commands;
using Ampere.Application.SignalR.Notifications;
using Ampere.Application.SignalR.Queries;
using Ampere.Application.SignalR.Responses;
using Ampere.Application.SignalR.Validators;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ampere.UnitTests.SignalR;

/// <summary>Validates end-to-end Mediator dispatch.</summary>
public sealed class SignalRDispatchTests
{
    [Fact]
    public async Task DiscoveryCommand_DispatchesToHandler()
    {
        IMediator mediator = CreateProvider()
            .GetRequiredService<IMediator>();

        Response<DiscoveryResponse> result =
            await mediator.Send(
                new StartDiscoveryCommand("house", 30),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("house", result.Data?.HouseId);
    }

    [Fact]
    public async Task RelayCommand_DispatchesToHandler()
    {
        IMediator mediator = CreateProvider()
            .GetRequiredService<IMediator>();

        Response<RelayStateResponse> result =
            await mediator.Send(
                new SetRelayCommand("endpoint", true),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(result.Data?.State);
    }

    [Fact]
    public async Task FirmwareCommand_DispatchesToHandler()
    {
        IMediator mediator = CreateProvider()
            .GetRequiredService<IMediator>();

        Response<FirmwareProgressResponse> result =
            await mediator.Send(
                new StartFirmwareUpdateCommand(
                    "node",
                    "2.0.0"),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("node", result.Data?.NodeId);
    }

    [Fact]
    public async Task Query_DispatchesToHandler()
    {
        IMediator mediator = CreateProvider()
            .GetRequiredService<IMediator>();

        Response<TelemetrySnapshotResponse> result =
            await mediator.Send(
                new GetTelemetrySnapshotQuery("house"),
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Data!.Items);
    }

    [Fact]
    public async Task Notification_DispatchesToHandler()
    {
        ServiceProvider provider = CreateProvider();
        IMediator mediator =
            provider.GetRequiredService<IMediator>();
        TelemetryResponse telemetry = CreateTelemetry();

        await mediator.Publish(
            new TelemetryUpdatedNotification(telemetry),
            CancellationToken.None);

        FakeSignalRService service =
            provider.GetRequiredService<FakeSignalRService>();
        Assert.Same(telemetry, service.Telemetry);
    }

    [Fact]
    public async Task Stream_DispatchesToHandler()
    {
        ServiceProvider provider = CreateProvider();
        IMediator mediator =
            provider.GetRequiredService<IMediator>();
        FakeSignalRService service =
            provider.GetRequiredService<FakeSignalRService>();
        TelemetryResponse telemetry = CreateTelemetry();
        service.StreamItems.Add(telemetry);

        List<TelemetryResponse> items = [];
        await foreach (TelemetryResponse item in mediator.CreateStream(
            new WatchTelemetryQuery("house"),
            CancellationToken.None))
        {
            items.Add(item);
        }

        Assert.Single(items);
        Assert.Same(telemetry, items[0]);
    }

    [Fact]
    public async Task Command_InvalidInput_IsRejectedByPipeline()
    {
        IMediator mediator = CreateProvider()
            .GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ValidationException>(
            async () => await mediator.Send(
                new SetRelayCommand(string.Empty, true),
                CancellationToken.None));
    }

    private static ServiceProvider CreateProvider()
    {
        ServiceCollection services = new();
        services.AddSingleton<FakeSignalRService>();
        services.AddSingleton<ISignalRService>(
            provider => provider.GetRequiredService<
                FakeSignalRService>());
        services.AddSingleton<
            IValidator<SetRelayCommand>,
            SetRelayCommandValidator>();
        services.AddMediator(options =>
        {
            options.Assemblies =
            [typeof(StartDiscoveryCommand).Assembly];
            options.PipelineBehaviors =
            [typeof(ValidationMiddleware<,>)];
        });
        return services.BuildServiceProvider();
    }

    private static TelemetryResponse CreateTelemetry()
    {
        return new TelemetryResponse(
            "house",
            "node",
            "endpoint",
            DateTimeOffset.UtcNow,
            220,
            1,
            220,
            100,
            true);
    }

    private sealed class FakeSignalRService
        : ISignalRService
    {
        public TelemetryResponse? Telemetry { get; private set; }

        public List<TelemetryResponse> StreamItems { get; } = [];

        public Task NotifyDiscoveryAsync(
            DiscoveryResponse response,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task NotifyRelayStateAsync(
            RelayStateResponse response,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task NotifyFirmwareProgressAsync(
            FirmwareProgressResponse response,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task PublishTelemetryAsync(
            TelemetryResponse response,
            CancellationToken cancellationToken)
        {
            Telemetry = response;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TelemetryResponse>>
            GetTelemetrySnapshotAsync(
                string? houseId,
                CancellationToken cancellationToken)
        {
            return Task.FromResult(
                (IReadOnlyList<TelemetryResponse>)[]);
        }

        public async IAsyncEnumerable<TelemetryResponse>
            WatchTelemetryAsync(
                string? houseId,
                [System.Runtime.CompilerServices.EnumeratorCancellation]
                CancellationToken cancellationToken)
        {
            foreach (TelemetryResponse item in StreamItems)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return item;
            }
        }
    }
}
