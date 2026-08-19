using Ampere.Application.Identity.Abstractions;
using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Identity.Services;
using Ampere.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ampere.UnitTests.Common.ConfiguredFixtures;

/// <summary>Builds a fully configured in-memory Identity fixture.</summary>
public sealed class IdentityTestFixture : IDisposable
{
    private readonly ServiceProvider provider;

    /// <summary>Initializes the Identity test fixture.</summary>
    public IdentityTestFixture()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddDbContext<AmpereDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
            options.SignIn.RequireConfirmedEmail = false;
        }).AddRoles<Role>()
        .AddEntityFrameworkStores<AmpereDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();
        services.AddScoped<IIdentityEmailSender,
            TestIdentityEmailSender>();
        services.Configure<
            Ampere.Infrastructure.Identity.Options.JwtOptions>(
            options =>
            {
                options.Key =
                    "01234567890123456789012345678901";
                options.Issuer = "Ampere.Tests";
                options.Audience = "Ampere.Tests";
            });
        services.AddSingleton<IRevokedTokenStore,
            RevokedTokenStore>();
        services.AddScoped<IJwtTokenService,
            JwtTokenService>();
        services.AddScoped<IdentityService>();
        provider = services.BuildServiceProvider();
    }

    /// <summary>Gets the Identity service under test.</summary>
    public IdentityService Service =>
        provider.GetRequiredService<IdentityService>();

    /// <summary>Gets the user manager.</summary>
    public UserManager<User> UserManager =>
        provider.GetRequiredService<UserManager<User>>();

    /// <summary>Gets the role manager.</summary>
    public RoleManager<Role> RoleManager =>
        provider.GetRequiredService<RoleManager<Role>>();

    /// <summary>Creates and stores a user.</summary>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>The created user.</returns>
    public async Task<User> CreateUserAsync(
        string email,
        string password = "Password1!")
    {
        User user = new(email)
        {
            Email = email,
            EmailConfirmed = true
        };
        IdentityResult result = await UserManager.CreateAsync(
            user,
            password);
        Assert.True(result.Succeeded);
        return user;
    }

    /// <summary>
    /// Generates an authenticator code and verifies it through the same
    /// configured Identity token provider used by the service under test.
    /// </summary>
    /// <param name="user">The user for whom the code is generated.</param>
    /// <returns>A code accepted by the configured authenticator provider.</returns>
    public async Task<string> GenerateValidAuthenticatorCodeAsync(User user)
    {
        string? key = await UserManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            IdentityResult resetResult =
                await UserManager.ResetAuthenticatorKeyAsync(user);
            Assert.True(resetResult.Succeeded);
        }

        string providerName = UserManager.Options.Tokens
            .AuthenticatorTokenProvider;

        for (int attempt = 0; attempt < 5; attempt++)
        {
            string code = await UserManager.GenerateTwoFactorTokenAsync(
                user,
                providerName);

            if (await UserManager.VerifyTwoFactorTokenAsync(
                    user,
                    providerName,
                    code))
            {
                return code;
            }

            await Task.Delay(50);
        }

        throw new InvalidOperationException(
            "The configured Identity authenticator token provider did not accept a token it generated.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        provider.Dispose();
    }

    private sealed class TestIdentityEmailSender :
        IIdentityEmailSender
    {
        public Task SendConfirmationAsync(
            string email,
            string confirmationLink,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetAsync(
            string email,
            string resetLink,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
