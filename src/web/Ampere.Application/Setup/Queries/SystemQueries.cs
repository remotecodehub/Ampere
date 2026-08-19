using Ampere.Application.Setup.Responses;
using Mediator;

namespace Ampere.Application.Setup.Queries;

/// <summary>Requests the current setup status.</summary>
public sealed record GetSetupStatusQuery
    : IRequest<SetupStatusResponse>;
