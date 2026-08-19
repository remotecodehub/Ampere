using Ampere.Application.Common.Responses;
using Ampere.Application.SignalR.Responses;
using Mediator;

namespace Ampere.Application.SignalR.Commands;

/// <summary>Requests a relay state change.</summary>
public sealed record SetRelayCommand(
    string EndpointId,
    bool State)
    : IRequest<Response<RelayStateResponse>>;
