namespace Ampere.Application.SignalR.Requests;

/// <summary>Requests a Sonoff firmware update.</summary>
/// <param name="NodeId">The radio node identifier.</param>
/// <param name="Version">The firmware version.</param>
public sealed record FirmwareUpdateRequest(
    string NodeId,
    string Version);
