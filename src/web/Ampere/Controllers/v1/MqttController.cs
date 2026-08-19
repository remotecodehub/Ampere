using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Mediator.Net;
using Microsoft.AspNetCore.Mvc;

namespace Ampere.Controllers.v1;

/// <summary>
/// Exposes REST endpoints to manage the local MQTT broker.
/// Thin controller: delegates to mediator requests and
/// does not contain business logic.
/// </summary>
[ApiController]
public sealed class MqttController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets the current broker status.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The broker status.</returns>
    [HttpGet("/mqtt/status")]
    [ProducesResponseType<Response<BrokerStatusResponse>>(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<GetBrokerStatusQuery, Response<BrokerStatusResponse>>(new GetBrokerStatusQuery(), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : StatusCode(500, result);
    }

    /// <summary>Gets the persisted broker configuration.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored configuration when present.</returns>
    [HttpGet("/mqtt/configuration")]
    [ProducesResponseType(typeof(Response<BrokerConfigurationResponse?>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetConfiguration(CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<GetBrokerConfigurationQuery, Response<BrokerConfigurationResponse?>>(new GetBrokerConfigurationQuery(), cancellationToken);
        return result.Data is null ? NotFound(result) : Ok(result.Data);
    }

    /// <summary>Persists and applies broker configuration.</summary>
    /// <param name="request">Configuration payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored configuration.</returns>
    [HttpPost("/mqtt/configuration")]
    [ProducesResponseType<Response<BrokerConfigurationResponse>>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Configure(ConfigureBrokerRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<ConfigureBrokerCommand, Response<BrokerConfigurationResponse>>(new ConfigureBrokerCommand(request), cancellationToken);
        return result.Succeeded ? Created(string.Empty, result.Data) : BadRequest(result);
    }

    /// <summary>Starts the broker.</summary>
    [HttpPost("/mqtt/start")]
    [ProducesResponseType<Response<BrokerStatusResponse>>(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Start(CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<StartBrokerCommand, Response<BrokerStatusResponse>>(new StartBrokerCommand(), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : StatusCode(500, result);
    }

    /// <summary>Stops the broker.</summary>
    [HttpPost("/mqtt/stop")]
    [ProducesResponseType(typeof(Response<BrokerStatusResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Stop(CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<StopBrokerCommand, Response<BrokerStatusResponse>>(new StopBrokerCommand(), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : StatusCode(500, result);
    }

    /// <summary>Publishes a message to a topic.</summary>
    /// <param name="request">Publish payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("/mqtt/publish")]
    [ProducesResponseType(typeof(Response<bool>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Publish(PublishMessageRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.RequestAsync<PublishMessageCommand, Response<bool>>(new PublishMessageCommand(request), cancellationToken);
        return response.Succeeded ? Ok(response) : BadRequest(response);
    }
}
