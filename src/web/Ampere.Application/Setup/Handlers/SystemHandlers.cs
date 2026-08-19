using Ampere.Application.Identity.Responses;
using Ampere.Application.Setup.Abstractions;
using Ampere.Application.Setup.Commands;
using Ampere.Application.Setup.Queries;
using Ampere.Application.Setup.Responses;
using Mediator;

namespace Ampere.Application.Setup.Handlers;

/// <summary>Handles setup messages.</summary>
/// <param name="service">The setup service.</param>
public sealed class SystemHandlers(
    ISystemSetupService service)
    : IRequestHandler<GetSetupStatusQuery,
        SetupStatusResponse>,
      IRequestHandler<InitializeSetupCommand,
        IdentityResultResponse>
{
    /// <inheritdoc />
    public ValueTask<SetupStatusResponse> Handle(
        GetSetupStatusQuery request,
        CancellationToken cancellationToken)
    {
        return new ValueTask<SetupStatusResponse>(
            service.GetSetupStatusAsync(
                cancellationToken));
    }

    /// <inheritdoc />
    public ValueTask<IdentityResultResponse> Handle(
        InitializeSetupCommand request,
        CancellationToken cancellationToken)
    {
        return new ValueTask<IdentityResultResponse>(
            service.InitializeSetupAsync(
                request.Email,
                request.Password,
                cancellationToken));
    }
}
