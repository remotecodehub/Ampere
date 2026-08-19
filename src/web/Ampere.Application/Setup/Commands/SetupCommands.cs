using Ampere.Application.Identity.Responses;
using Mediator;

namespace Ampere.Application.Setup.Commands;

/// <summary>Requests initial administrator creation.</summary>
/// <param name="Email">The administrator email.</param>
/// <param name="Password">The administrator password.</param>
public sealed record InitializeSetupCommand(
    string Email,
    string Password)
    : IRequest<IdentityResultResponse>;
