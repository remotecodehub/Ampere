using Mediator.Net.Contracts;

namespace Ampere.Application.Identity.Queries;

/// <summary>Requests basic identity information for a user.</summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetIdentityInfoQuery(string UserId) : IRequest;
