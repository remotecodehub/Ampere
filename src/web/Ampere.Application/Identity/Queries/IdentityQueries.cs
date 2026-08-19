using Ampere.Application.Common.Responses;
using Ampere.Application.Identity.Responses;
using Mediator;

namespace Ampere.Application.Identity.Queries;

/// <summary>Requests identity information.</summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetIdentityInfoQuery(
    string UserId)
    : IRequest<Response<IdentityInfoResponse>>;
