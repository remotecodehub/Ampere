using Ampere.Application.MQTT.Abstractions;
using System.Reflection;
using System.Linq;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Ampere.Infrastructure.MQTT.Models;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace Ampere.Infrastructure.MQTT.Services;

/// <summary>
/// Concrete implementation of the broker service using
/// MQTTnet. This type remains in Infrastructure and does
/// not leak MQTTnet types to Application.
/// </summary>
/// <remarks>Constructs the broker service.</remarks>
public sealed class MqttBrokerService(IServiceProvider services, Ampere.Infrastructure.Persistence.AmpereDbContext dbContext) : IMqttBrokerService, IAsyncDisposable
{
    private readonly IServiceProvider _services = services;
    private readonly Ampere.Infrastructure.Persistence.AmpereDbContext _dbContext = dbContext;
    private dynamic? _server;
    private DateTimeOffset? _startedAt;

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_server is not null && _server.IsStarted)
        {
            return;
        }

        MqttBrokerConfigurationEntity? cfg = await _dbContext.MqttBrokerConfigurations.OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(cancellationToken);

        // Use reflection to avoid compile-time dependency on
        // MQTTnet symbols while still leveraging the runtime
        // package when available.
        var mqttAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.StartsWith("MQTTnet", StringComparison.OrdinalIgnoreCase) == true)
            ?? (Assembly.Load("MQTTnet") ?? Assembly.Load("MQTTnet.AspNetCore"));

        if (mqttAssembly is null)
        {
            // MQTTnet not available at runtime; mark started false.
            return;
        }

        Type? factoryType = mqttAssembly.GetTypes().FirstOrDefault(t => t.Name == "MqttFactory" || t.Name == "MqttServerFactory");
        if (factoryType is null)
        {
            return;
        }

        var factory = Activator.CreateInstance(factoryType);
        var createServerMethod = factoryType.GetMethod("CreateMqttServer", Type.EmptyTypes)
                                 ?? factoryType.GetMethod("CreateServer", Type.EmptyTypes);
        if (createServerMethod is null)
        {
            return;
        }

        _server = createServerMethod.Invoke(factory, null);

        // Prefer parameterless StartAsync when available.
        MethodInfo? startMethod = _server.GetType().GetMethod("StartAsync", Type.EmptyTypes)
            ?? _server.GetType().GetMethod("StartAsync", new[] { typeof(object) });

        if (startMethod is not null)
        {
            var task = (Task)startMethod.Invoke(_server, null)!;
            await task.ConfigureAwait(false);
            _startedAt = DateTimeOffset.UtcNow;
        }
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_server is null)
        {
            return;
        }

        await _server.StopAsync();
        _server = null;
        _startedAt = null;
    }

    /// <inheritdoc/>
    public async Task RestartAsync(CancellationToken cancellationToken)
    {
        await StopAsync(cancellationToken);
        await StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<BrokerStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        bool running = _server is not null && _server.IsStarted;
        int count = 0;
        int port = 0;
        string? bind = null;

        if (running && _server is not null)
        {
            var clients = await _server.GetConnectedClientsAsync();
            count = (int)clients.Count;
        }

        var cfg = await _dbContext.MqttBrokerConfigurations.OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(cancellationToken);
        if (cfg is not null)
        {
            port = cfg.Port;
            bind = cfg.BindAddress;
        }

        return new BrokerStatusResponse(running, _startedAt, port, bind, count);
    }

    /// <inheritdoc/>
    public async Task PublishAsync(string topic, byte[] payload, CancellationToken cancellationToken)
    {
        if (_server is null || !_server.IsStarted)
        {
            throw new InvalidOperationException("MQTT broker is not running.");
        }

        // Build the application message via reflection to avoid
        // compile-time dependency and then inject it into the
        // runtime server instance.
        var mqttAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.StartsWith("MQTTnet", StringComparison.OrdinalIgnoreCase) == true);

        if (mqttAssembly is null)
        {
            throw new InvalidOperationException("MQTTnet assembly not available at runtime.");
        }

        Type? builderType = mqttAssembly.GetTypes().FirstOrDefault(t => t.Name == "MqttApplicationMessageBuilder" || t.Name == "MqttApplicationMessageBuilder`1" );
        if (builderType is null)
        {
            throw new InvalidOperationException("MQTT message builder type not found.");
        }

        var builder = Activator.CreateInstance(builderType);
        MethodInfo? withTopic = builderType.GetMethod("WithTopic", new[] { typeof(string) });
        MethodInfo? withPayload = builderType.GetMethod("WithPayload", new[] { typeof(byte[]) });
        MethodInfo? withQos = builderType.GetMethod("WithQualityOfServiceLevel", new[] { mqttAssembly.GetTypes().FirstOrDefault(t => t.Name.Contains("MqttQualityOfServiceLevel") ) ?? typeof(object) });
        MethodInfo? build = builderType.GetMethod("Build", Type.EmptyTypes);

        withTopic?.Invoke(builder, new object[] { topic });
        withPayload?.Invoke(builder, new object[] { payload });
        var message = build?.Invoke(builder, null);

        // Attempt to inject the message
        var injectedType = mqttAssembly.GetTypes().FirstOrDefault(t => t.Name == "InjectedMqttApplicationMessage");
        object? injected = null;
        if (injectedType is not null)
        {
            var ctor = injectedType.GetConstructors().FirstOrDefault();
            injected = ctor?.Invoke(new[] { message });
        }

        MethodInfo? injectMethod = _server.GetType().GetMethod("InjectApplicationMessage")
            ?? _server.GetType().GetMethod("InjectMessage");

        if (injectMethod is not null)
        {
            if (injected is not null)
            {
                var task = (Task)injectMethod.Invoke(_server, new[] { injected })!;
                await task.ConfigureAwait(false);
                return;
            }

            // Fallback to PublishAsync if available
            var publishMethod = _server.GetType().GetMethod("PublishAsync", new[] { message?.GetType() }) ?? _server.GetType().GetMethod("PublishAsync", new[] { typeof(object) });
            if (publishMethod is not null)
            {
                var task = (Task)publishMethod.Invoke(_server, new[] { message })!;
                await task.ConfigureAwait(false);
                return;
            }
        }

        throw new InvalidOperationException("Unable to publish message: MQTT server does not expose injection or publish methods.");
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_server is not null)
        {
            await _server.StopAsync();
            _server = null;
        }
    }
}
