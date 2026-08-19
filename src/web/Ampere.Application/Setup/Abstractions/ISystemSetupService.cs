using Ampere.Application.Identity.Responses;
using Ampere.Application.Setup.Responses;

namespace Ampere.Application.Setup.Abstractions;

public interface ISystemSetupService
{
 
    /// <summary>Creates the initial administrator account when setup has not yet completed.</summary>
    /// <param name="email">The administrator email address.</param>
    /// <param name="password">The administrator password.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The setup operation result.</returns>
    Task<IdentityResultResponse> InitializeSetupAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>Gets the current first-time setup status.</summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The setup status.</returns>
    Task<SetupStatusResponse> GetSetupStatusAsync(CancellationToken cancellationToken);
   
}
