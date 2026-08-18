using System.Security.Claims;
using Ampere.Application.Common.Responses;
using Ampere.Application.Identity.Commands;
using Ampere.Application.Identity.Queries;
using Ampere.Application.Identity.Requests;
using Ampere.Application.Identity.Responses;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
namespace Ampere.Controllers.v1;

/// <summary>Exposes HTTP endpoints for registration, authentication, account management, and two-factor authentication.</summary>
/// <param name="mediator">The mediator used to dispatch application requests.</param>
[ApiController]
public sealed class IdentityController(IMediator mediator) : ControllerBase
{
    /// <summary>Registers a new user.</summary>
    /// <param name="request">The registration payload.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response representing the registration result.</returns>
    [HttpPost("/register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<RegisterCommand, IdentityResultResponse>(new RegisterCommand(request.Email, request.Password), cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    /// <summary>Authenticates a user with a password and optional second factor.</summary>
    /// <param name="request">The login payload.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response containing tokens when authentication succeeds.</returns>
    [HttpPost("/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<LoginCommand, Response<TokenResponse>>(new LoginCommand(request.Email, request.Password, request.TwoFactorCode, request.TwoFactorRecoveryCode), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : Unauthorized(result);
    }

    /// <summary>Exchanges a refresh token for a new token pair.</summary>
    /// <param name="request">The refresh-token payload.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response containing the refreshed tokens.</returns>
    [HttpPost("/refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<RefreshTokenCommand, Response<TokenResponse>>(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : Unauthorized(result);
    }

    /// <summary>Revokes the access token supplied by the authenticated caller.</summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response indicating whether revocation succeeded.</returns>
    [HttpPost("/revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
    {
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
        var result = await mediator.RequestAsync<RevokeTokenCommand, Response<bool>>(new RevokeTokenCommand(accessToken), cancellationToken);
        return result.Data == true ? Ok() : Unauthorized(result);
    }

    /// <summary>Confirms a user's email address.</summary>
    /// <param name="userId">The user identifier from the confirmation link.</param>
    /// <param name="code">The confirmation token.</param>
    /// <param name="changedEmail">The replacement email address when confirming an email change.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response indicating whether confirmation succeeded.</returns>
    [HttpGet("/confirmEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<ConfirmEmailCommand, Response<bool>>(
            new ConfirmEmailCommand(userId, code, changedEmail),
            cancellationToken);
        return result.Data == true ? Ok("Thank you for confirming your email.") : BadRequest(result);
    }

    /// <summary>Resends a user's email confirmation link.</summary>
    /// <param name="request">The email payload.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response representing the resend result.</returns>
    [HttpPost("/resendConfirmationEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmationEmail(EmailRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator
            .RequestAsync<ResendConfirmationEmailCommand, IdentityResultResponse>(
                new ResendConfirmationEmailCommand(request.Email),
                cancellationToken);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    /// <summary>Starts password recovery for an email address.</summary>
    /// <param name="request">The email payload.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response representing the recovery result.</returns>
    [HttpPost("/forgotPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(EmailRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<ForgotPasswordCommand, IdentityResultResponse>(new ForgotPasswordCommand(request.Email), cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    /// <summary>Resets a user's password using a reset token.</summary>
    /// <param name="request">The password reset payload.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response representing the reset result.</returns>
    [HttpPost("/resetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<ResetPasswordCommand, IdentityResultResponse>(new ResetPasswordCommand(request.Email, request.ResetCode, request.NewPassword), cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    /// <summary>Gets identity information for the authenticated user.</summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response containing identity information.</returns>
    [HttpGet("/manage/info")]
    [Authorize]
    public async Task<IActionResult> GetInfo(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.RequestAsync<GetIdentityInfoQuery, Response<IdentityInfoResponse>>(new GetIdentityInfoQuery(userId), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : NotFound(result);
    }

    /// <summary>Updates identity information for the authenticated user.</summary>
    /// <param name="request">The identity update payload.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response representing the update result.</returns>
    [HttpPost("/manage/info")]
    [Authorize]
    public async Task<IActionResult> UpdateInfo(InfoRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.RequestAsync<UpdateIdentityInfoCommand, IdentityResultResponse>(new UpdateIdentityInfoCommand(userId, request.NewEmail, request.NewPassword, request.OldPassword), cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    /// <summary>Configures authenticator-based two-factor authentication for the authenticated user.</summary>
    /// <param name="request">The two-factor configuration payload.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>An HTTP response containing the resulting two-factor configuration.</returns>
    [HttpPost("/manage/2fa")]
    [Authorize]
    public async Task<IActionResult> ConfigureTwoFactor(TwoFactorRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.RequestAsync<ConfigureTwoFactorCommand, Response<TwoFactorResponse>>(new ConfigureTwoFactorCommand(userId, request.Enable, request.TwoFactorCode, request.ResetRecoveryCodes, request.ResetSharedKey, request.ForgetMachine), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result);
    }

    private string? GetUserId() => User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

