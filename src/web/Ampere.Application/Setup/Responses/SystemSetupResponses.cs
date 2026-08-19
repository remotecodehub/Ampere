using Mediator.Net.Contracts;

namespace Ampere.Application.Setup.Responses;

/// <summary>Represents the current first-time setup status of the application.</summary>
/// <param name="IsSetupRequired">Indicates whether initial setup is still required.</param>
/// <param name="IsSetupComplete">Indicates whether initial setup has completed.</param>
public sealed record SetupStatusResponse(bool IsSetupRequired, bool IsSetupComplete) : IResponse;
