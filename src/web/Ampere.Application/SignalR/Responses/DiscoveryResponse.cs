namespace Ampere.Application.SignalR.Responses;

/// <summary>Describes a discovery state notification.</summary>
public sealed record DiscoveryResponse(
    string HouseId,
    int WindowSeconds,
    bool Active);
