using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Pipeline.Validation;
using Ampere.Application.Identity.Abstractions;
using Ampere.Application.Identity.Handlers;
using Ampere.Application.SignalR.Handlers;
using Ampere.Infrastructure.Common.Hubs;
using Ampere.Infrastructure.Common.Repository;
using Ampere.Infrastructure.Common.UnitOfWork;
using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Identity.Options;
using Ampere.Infrastructure.Identity.Services;
using Ampere.Infrastructure.Persistence;
using Ampere.Infrastructure.Persistence.Middlewares;
using Ampere.UnitTests.Common.Mocks;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ampere.UnitTests.Common.Fixtures;

/// <summary>Builds an in-memory SignalR service.</summary>
public sealed class AmpereTestFixture : IDisposable
{
    private readonly ServiceProvider _provider;
    public ServiceProvider Services { get; init; }
    /// <summary>Initializes the Identity test fixture.</summary>
    public AmpereTestFixture()
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
            JwtOptions>(
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<FakeSignalRService>();
        services.AddScoped<ISignalRService, SignalRService>();
        services.AddScoped(typeof(IRepository<>), typeof(FakeRepository<>));
        services.AddScoped(sp 
            => new SignalRHandlers(
                sp.GetRequiredService<ISignalRService>()));
        services.AddMediator(options =>
        {
            options.ServiceLifetime =
                ServiceLifetime.Scoped;
            options.Assemblies =
                [typeof(IdentityHandlers).Assembly];
            options.PipelineBehaviors =
            [
                typeof(ValidationMiddleware<,>),
                typeof(TransactionMiddleware<,>)
            ];
        });
        _provider = services.BuildServiceProvider();
        Services = _provider;
    }
    /// <summary>Gets the IMediator service under test.</summary>
    public IMediator Mediator => _provider.GetRequiredService<IMediator>();

    /// <summary>Gets the UnitOfWork service under test.</summary>
    public IUnitOfWork UnitOfWork => _provider.GetRequiredService<IUnitOfWork>();

    /// <summary>Gets the SignalR service under test.</summary>
    public FakeSignalRService FSignalR => _provider.GetRequiredService<FakeSignalRService>();

    /// <summary>Gets the Identity service under test.</summary>
    public IdentityService Identity => _provider.GetRequiredService<IdentityService>();

    /// <summary>Gets the database context.</summary>
    public AmpereDbContext DbContext =>
        _provider.GetRequiredService<AmpereDbContext>();

    /// <summary>Gets the user manager.</summary>
    public UserManager<User> UserManager =>
        _provider.GetRequiredService<UserManager<User>>();

    /// <summary>Gets the role manager.</summary>
    public RoleManager<Role> RoleManager =>
        _provider.GetRequiredService<RoleManager<Role>>();

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
