using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Responses;
using Mediator;

namespace Ampere.Application.SignalR.Commands;

/// <summary>Starts device discovery.</summary>
public sealed record StartDiscoveryCommand(
    string HouseId,
    int WindowSeconds)
    : IRequest<Response<DiscoveryResponse>>;
