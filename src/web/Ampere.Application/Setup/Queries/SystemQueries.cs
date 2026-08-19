using Mediator.Net.Contracts;

namespace Ampere.Application.Setup.Queries;

/// <summary>Requests the current first-time setup status.</summary>
public sealed record GetSetupStatusQuery : IRequest;
