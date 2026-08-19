using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Pipeline.Validation;
using Ampere.Application.Identity.Abstractions;
using Ampere.Application.Identity.Handlers;
using Ampere.Application.SignalR.Commands;
using Ampere.Application.SignalR.Handlers;
using Ampere.Application.SignalR.Validators;
using Ampere.Infrastructure.Common.Hubs;
using Ampere.Infrastructure.Common.Repository;
using Ampere.Infrastructure.Common.UnitOfWork;
using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Identity.Options;
using Ampere.Infrastructure.Identity.Services;
using Ampere.Infrastructure.Persistence;
using Ampere.Infrastructure.Persistence.Middlewares;
using Ampere.UnitTests.Common.Mocks;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ampere.UnitTests.Common.Fixtures;

/// <summary>Builds an in-memory application service provider for integration-style unit tests.</summary>
public sealed class AmpereTestFixture : IDisposable
{
    private readonly ServiceProvider _provider;

    /// <summary>Initializes the test fixture and its dependency injection container.</summary>
    public AmpereTestFixture()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddSignalR();
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
        services.Configure<JwtOptions>(options =>
        {
            options.Key = "01234567890123456789012345678901";
            options.Issuer = "Ampere.Tests";
            options.Audience = "Ampere.Tests";
        });
        services.AddSingleton<IRevokedTokenStore, RevokedTokenStore>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IdentityService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register the validators consumed by ValidationMiddleware, matching the
        // application composition root. Without this registration the middleware
        // receives an empty validator collection and invalid commands reach handlers.
        services.AddScoped<IValidator<SetRelayCommand>, SetRelayCommandValidator>();

        // Dispatch tests intentionally use a deterministic fake at the application boundary.
        // The concrete SignalR implementation is also registered so it can be resolved and tested
        // independently with the real IHubContext supplied by AddSignalR().
        services.AddScoped<FakeSignalRService>();
        services.AddScoped<ISignalRService>(sp =>
            sp.GetRequiredService<FakeSignalRService>());
        services.AddScoped<SignalRService>();

        services.AddScoped(typeof(IRepository<>), typeof(FakeRepository<>));
        services.AddScoped(sp =>
            new SignalRHandlers(
                sp.GetRequiredService<ISignalRService>()));
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(IdentityHandlers).Assembly];
            options.PipelineBehaviors =
            [
                typeof(ValidationMiddleware<,>),
                typeof(TransactionMiddleware<,>)
            ];
        });

        _provider = services.BuildServiceProvider();
    }

    /// <summary>Gets the service provider used by the fixture.</summary>
    public ServiceProvider Services => _provider;

    /// <summary>Gets the IMediator service under test.</summary>
    public IMediator Mediator => _provider.GetRequiredService<IMediator>();

    /// <summary>Gets the UnitOfWork service under test.</summary>
    public IUnitOfWork UnitOfWork => _provider.GetRequiredService<IUnitOfWork>();

    /// <summary>Gets the fake SignalR boundary used by dispatch tests.</summary>
    public FakeSignalRService FSignalR =>
        _provider.GetRequiredService<FakeSignalRService>();

    /// <summary>Gets the concrete SignalR service used by implementation tests.</summary>
    public SignalRService SignalR =>
        _provider.GetRequiredService<SignalRService>();

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
        IdentityResult result = await UserManager.CreateAsync(user, password);
        Assert.True(result.Succeeded);
        return user;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _provider.Dispose();
    }

    private sealed class TestIdentityEmailSender : IIdentityEmailSender
    {
        public Task SendConfirmationAsync(
            string email,
            string confirmationLink,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task SendPasswordResetAsync(
            string email,
            string resetLink,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
