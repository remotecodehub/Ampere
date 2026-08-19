namespace Ampere.Application.SignalR.Requests;

/// <summary>Starts a Sonoff discovery window.</summary>
/// <param name="HouseId">The target house identifier.</param>
/// <param name="WindowSeconds">The discovery duration.</param>
public sealed record StartDiscoveryRequest(
    string HouseId,
    int WindowSeconds);
