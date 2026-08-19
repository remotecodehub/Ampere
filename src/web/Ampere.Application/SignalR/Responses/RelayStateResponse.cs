namespace Ampere.Application.SignalR.Responses;

/// <summary>Describes a relay state change.</summary>
public sealed record RelayStateResponse(
    string EndpointId,
    bool State);
