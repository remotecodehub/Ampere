using Ampere.Application.Common.Responses;
using Ampere.Application.MQTT.Commands;
using Ampere.Application.MQTT.Queries;
using Ampere.Application.MQTT.Requests;
using Ampere.Application.MQTT.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Ampere.Controllers.v1;

/// <summary>Exposes MQTT broker operations.</summary>
/// <param name="mediator">The application mediator.</param>
[ApiController]
[Route("mqtt")]
public sealed class MqttController(
    IMediator mediator) : ControllerBase
{
    /// <summary>Gets broker status.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The broker status.</returns>
    [HttpGet("status")]
    [ProducesResponseType<BrokerStatusResponse>(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetStatus(
        CancellationToken cancellationToken)
    {
        BrokerStatusResponse result =
            await mediator.Send(
                new GetBrokerStatusQuery(),
                cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets persisted broker configuration.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The broker configuration.</returns>
    [HttpGet("configuration")]
    [ProducesResponseType<BrokerConfigurationResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetConfiguration(
        CancellationToken cancellationToken)
    {
        Response<BrokerConfigurationResponse?> result =
            await mediator.Send(
                new GetBrokerConfigurationQuery(),
                cancellationToken);

        return result.Data is null
            ? NotFound(result)
            : Ok(result.Data);
    }

    /// <summary>Saves and applies broker configuration.</summary>
    /// <param name="request">The broker configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The saved configuration.</returns>
    [HttpPost("configuration")]
    [ProducesResponseType<BrokerConfigurationResponse>(200)]
    [ProducesResponseType<Response<BrokerConfigurationResponse>>(400)]
    public async Task<IActionResult> Configure(
        ConfigureBrokerRequest request,
        CancellationToken cancellationToken)
    {
        Response<BrokerConfigurationResponse> result =
            await mediator.Send(
                new ConfigureBrokerCommand(request),
                cancellationToken);

        return result.Succeeded
            ? Ok(result.Data)
            : BadRequest(result);
    }

    /// <summary>Starts the broker.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The broker status.</returns>
    [HttpPost("start")]
    [ProducesResponseType<BrokerStatusResponse>(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Start(
        CancellationToken cancellationToken)
    {
        BrokerStatusResponse result =
            await mediator.Send(
                new StartBrokerCommand(),
                cancellationToken);
        return Ok(result);
    }

    /// <summary>Stops the broker.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The broker status.</returns>
    [HttpPost("stop")]
    [ProducesResponseType<BrokerStatusResponse>(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Stop(
        CancellationToken cancellationToken)
    {
        BrokerStatusResponse result =
            await mediator.Send(
                new StopBrokerCommand(),
                cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets connected MQTT clients.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connected clients.</returns>
    [HttpGet("clients")]
    [ProducesResponseType<
        IReadOnlyList<MqttClientResponse>>(200)]
    public async Task<IActionResult> GetClients(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<MqttClientResponse> result =
            await mediator.Send(
                new GetConnectedClientsQuery(),
                cancellationToken);
        return Ok(result);
    }

    /// <summary>Publishes a message to a topic.</summary>
    /// <param name="request">The publish request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The publish result.</returns>
    [HttpPost("publish")]
    [ProducesResponseType<Response<bool>>(200)]
    [ProducesResponseType<Response<bool>>(400)]
    public async Task<IActionResult> Publish(
        PublishMessageRequest request,
        CancellationToken cancellationToken)
    {
        Response<bool> result = await mediator.Send(
            new PublishMessageCommand(request),
            cancellationToken);
        return result.Succeeded
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Streams live MQTT messages.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The live MQTT message stream.</returns>
    [HttpGet("stream")]
    [Produces("application/json")]
    [ProducesResponseType<
        IAsyncEnumerable<MqttTopicMessageResponse>>(200)]
    public IAsyncEnumerable<MqttTopicMessageResponse> Stream(
        CancellationToken cancellationToken)
    {
        return mediator.CreateStream(
            new WatchTopicsQuery(),
            cancellationToken);
    }
}
