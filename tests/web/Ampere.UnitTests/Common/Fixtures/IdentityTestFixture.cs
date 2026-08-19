using Ampere.Application.Identity.Abstractions;
using Ampere.Application.Identity.Responses;
using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Identity.Services;
using Ampere.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ampere.UnitTests.Common.Fixtures;

/// <summary>Builds an in-memory Identity service.</summary>
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
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
            options.SignIn.RequireConfirmedEmail = false;
        }).AddRoles<Role>()
        .AddEntityFrameworkStores<AmpereDbContext>()
        .AddSignInManager();
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

    /// <summary>Gets the database context.</summary>
    public AmpereDbContext DbContext =>
        provider.GetRequiredService<AmpereDbContext>();

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

    /// <summary>Creates a token pair for a synthetic user.</summary>
    /// <param name="userId">The synthetic user identifier.</param>
    /// <returns>The generated token pair.</returns>
    public TokenResponse CreateTokensForTest(string userId)
    {
        IJwtTokenService tokenService =
            provider.GetRequiredService<IJwtTokenService>();
        return tokenService.CreateTokens(
            userId,
            "synthetic@example.com",
            [],
            []);
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
