namespace Ampere.Application.SignalR.Requests;

/// <summary>Requests a relay state change.</summary>
/// <param name="EndpointId">The endpoint identifier.</param>
/// <param name="State">The desired relay state.</param>
public sealed record SetRelayRequest(
    string EndpointId,
    bool State);
