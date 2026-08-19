using System.Security.Claims;
using Ampere.Application.Common.Contracts;
using Ampere.Application.Identity.Responses;
using Ampere.Application.Setup.Abstractions;
using Ampere.Application.Setup.Responses;
using Ampere.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ampere.Infrastructure.Setup.Services;

public sealed class SystemSetupService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ILogger<SystemSetupService> logger)
: ISystemSetupService 
{
    private const string AdministratorRole = "Administrator";
    private const string AdministratorPermission = "system.admin";

    /// <summary>
    /// Gets whether initial system setup is required or has already been completed.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current setup status.</returns>
    public async Task<SetupStatusResponse> GetSetupStatusAsync(CancellationToken cancellationToken)
    {
        var hasUsers = await userManager.Users.AsNoTracking().AnyAsync(cancellationToken);
        return new SetupStatusResponse(!hasUsers, hasUsers);
    }

    /// <summary>
    /// Creates the initial administrator account when system setup has not yet been completed.
    /// </summary>
    /// <param name="email">The administrator email address.</param>
    /// <param name="password">The administrator password.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The result of the setup operation.</returns>
    public async Task<IdentityResultResponse> InitializeSetupAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (await userManager.Users.AsNoTracking().AnyAsync(cancellationToken))
        {
            return IdentityResultResponse.Failure(["The system setup has already been completed."]);
        }

        Role? role = await roleManager.FindByNameAsync(AdministratorRole);
        if (role is null)
        {
            role = new Role(AdministratorRole);
            IdentityResult roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                return Failure(roleResult);
            }

            IdentityResult claimResult = await roleManager.AddClaimAsync(role, new Claim(IdentityClaimTypes.Permission, AdministratorPermission));
            if (!claimResult.Succeeded)
            {
                return Failure(claimResult);
            }
        }

        var user = new User(email)
        {
            Email = email,
            EmailConfirmed = true
        };

        IdentityResult userResult = await userManager.CreateAsync(user, password);
        if (!userResult.Succeeded)
        {
            return Failure(userResult);
        }

        IdentityResult membershipResult = await userManager.AddToRoleAsync(user, AdministratorRole);
        if (!membershipResult.Succeeded)
        {
            return Failure(membershipResult);
        }

        logger.LogInformation("Initial system setup completed for user {UserId}.", user.Id);
        return IdentityResultResponse.Success();
    }
    private static IdentityResultResponse Failure(IdentityResult result) => IdentityResultResponse.Failure(result.Errors.Select(error => error.Description));

}
