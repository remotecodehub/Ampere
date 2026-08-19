using Ampere.Application.Identity.Commands;
using Ampere.Application.Identity.Responses;
using Ampere.Application.Setup.Commands;
using Ampere.Application.Setup.Queries;
using Ampere.Application.Setup.Requests;
using Ampere.Application.Setup.Responses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ampere.Controllers.v1;

/// <summary>Exposes first-time setup endpoints.</summary>
/// <param name="mediator">The application mediator.</param>
[ApiController]
[Route("api/v1/setup")]
public sealed class SetupController(
    IMediator mediator) : ControllerBase
{
    /// <summary>Gets setup status.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The setup status.</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType<SetupStatusResponse>(200)]
    public async Task<IActionResult> GetStatus(
        CancellationToken cancellationToken)
    {
        SetupStatusResponse result = await mediator.Send(
            new GetSetupStatusQuery(),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates the first administrator.</summary>
    /// <param name="request">The setup credentials.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The setup result.</returns>
    [HttpPost("initialize")]
    [AllowAnonymous]
    [ProducesResponseType<IdentityResultResponse>(200)]
    [ProducesResponseType<IdentityResultResponse>(409)]
    public async Task<IActionResult> Initialize(
        [FromBody] InitializeSetupRequest request,
        CancellationToken cancellationToken)
    {
        IdentityResultResponse result =
            await mediator.Send(
                new InitializeSetupCommand(
                    request.Email,
                    request.Password),
                cancellationToken);
        return result.Succeeded
            ? Ok(result)
            : Conflict(result);
    }
}
