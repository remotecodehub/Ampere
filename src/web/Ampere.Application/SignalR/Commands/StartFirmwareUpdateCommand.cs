using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Responses;
using Mediator;

namespace Ampere.Application.SignalR.Commands;

/// <summary>Starts a firmware update notification flow.</summary>
public sealed record StartFirmwareUpdateCommand(
    string NodeId,
    string Version)
    : IRequest<Response<FirmwareProgressResponse>>;
