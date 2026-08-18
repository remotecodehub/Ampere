using Ampere.Application.Identity.Responses;
using Ampere.Application.Setup.Abstractions;
using Ampere.Application.Setup.Commands;
using Ampere.Application.Setup.Queries;
using Ampere.Application.Setup.Responses;
using Mediator.Net.Context;
using Mediator.Net.Contracts;

namespace Ampere.Application.Setup.Handlers;

/// <summary>Handles the identity requests.</summary>
/// <param name="service">The system setup service that performs setup.</param>
public sealed class SystemHandlers(ISystemSetupService service) : 
IRequestHandler<GetSetupStatusQuery, SetupStatusResponse>,
IRequestHandler<InitializeSetupCommand, IdentityResultResponse>
{
    
    /// <summary>Reads the current setup status.</summary>
    /// <param name="context">The message context containing the query.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current setup status.</returns>
    public Task<SetupStatusResponse> Handle(IReceiveContext<GetSetupStatusQuery> context, CancellationToken cancellationToken) =>
        service.GetSetupStatusAsync(cancellationToken);


    /// <summary>Executes first-time setup initialization.</summary>
    /// <param name="context">The message context containing setup credentials.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The setup result.</returns>
    public Task<IdentityResultResponse> Handle(IReceiveContext<InitializeSetupCommand> context, CancellationToken cancellationToken) =>
        service.InitializeSetupAsync(context.Message.Email, context.Message.Password, cancellationToken);
}

