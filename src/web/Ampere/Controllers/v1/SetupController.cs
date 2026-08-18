using Ampere.Application.Identity.Commands;
using Ampere.Application.Identity.Responses;
using Ampere.Application.Setup.Commands;
using Ampere.Application.Setup.Queries;
using Ampere.Application.Setup.Requests;
using Ampere.Application.Setup.Responses;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ampere.Controllers.v1;

/// <summary>Exposes anonymous endpoints used during first-time application setup.</summary>
/// <param name="mediator">The mediator used to dispatch setup requests.</param>
[ApiController]
[Route("api/v1/setup")]
public sealed class SetupController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets the current first-time setup status.</summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The current setup status.</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken) =>
        Ok(await mediator.RequestAsync<GetSetupStatusQuery, SetupStatusResponse>(
            new GetSetupStatusQuery(),
            cancellationToken));

    /// <summary>Initializes the application with its first administrator account.</summary>
    /// <param name="request">The initial administrator credentials.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response indicating whether initialization succeeded.</returns>
    [HttpPost("initialize")]
    [AllowAnonymous]
    [ProducesResponseType<IdentityResultResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<IdentityResultResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Initialize([FromBody] InitializeSetupRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<InitializeSetupCommand, IdentityResultResponse>(
            new InitializeSetupCommand(request.Email, request.Password),
            cancellationToken);
        return result.Succeeded ? Ok(result) : Conflict(result);
    }
}

