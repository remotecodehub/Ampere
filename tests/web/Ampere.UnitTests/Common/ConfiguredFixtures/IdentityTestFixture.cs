using Ampere.Application.Identity.Abstractions;
using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Identity.Options;
using Ampere.Infrastructure.Identity.Services;
using Ampere.Infrastructure.Persistence;
using Ampere.UnitTests.Common.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ampere.UnitTests.Common.ConfiguredFixtures;

/// <summary>Builds a fully configured in-memory Identity fixture.</summary>
public sealed class IdentityTestFixture : IDisposable
{
    private readonly ServiceProvider _provider;

    internal CapturingEmailSender EmailSender { get; }

    /// <summary>Initializes the Identity test fixture.</summary>
    public IdentityTestFixture()
    {
        EmailSender = new CapturingEmailSender();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddAuthentication(options => options.DefaultScheme = IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme);
        services.AddDbContext<AmpereDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
        services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<Role>()
            .AddSignInManager<SignInManager<User>>()
            .AddEntityFrameworkStores<AmpereDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<JwtOptions>(options =>
        {
            options.Key = "ampere-test-secret-key-with-at-least-256-bits-2026";
            options.Issuer = "Ampere.Test";
            options.Audience = "Ampere.Test";
            options.AccessTokenLifetime = TimeSpan.FromMinutes(15);
            options.RefreshTokenLifetime = TimeSpan.FromDays(14);
        });
        services.AddSingleton<IRevokedTokenStore, RevokedTokenStore>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IIdentityEmailSender>(EmailSender);
        services.AddScoped<IdentityService>();
        
        _provider = services.BuildServiceProvider();
        _provider.GetRequiredService<AmpereDbContext>().Database.EnsureCreated();
    }
        
    public IdentityService Service => _provider.GetRequiredService<IdentityService>();
    public IJwtTokenService TokenService => _provider.GetRequiredService<IJwtTokenService>();
    public UserManager<User> UserManager => _provider.GetRequiredService<UserManager<User>>();
    public RoleManager<Role> RoleManager => _provider.GetRequiredService<RoleManager<Role>>();

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
        _provider.Dispose();
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
