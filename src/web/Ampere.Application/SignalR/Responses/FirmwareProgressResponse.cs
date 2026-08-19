namespace Ampere.Application.SignalR.Responses;

/// <summary>Describes firmware update progress.</summary>
public sealed record FirmwareProgressResponse(
    string NodeId,
    int Percent,
    string Message);
