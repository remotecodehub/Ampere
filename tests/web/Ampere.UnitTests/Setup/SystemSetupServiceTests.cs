using Ampere.Application.Identity.Responses;
using Ampere.Infrastructure.Setup.Services;
using Ampere.UnitTests.Common.Fixtures;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Ampere.UnitTests.Setup;

/// <summary>Tests initial system setup rules.</summary>
public sealed class SystemSetupServiceTests
{
    [Fact]
    public async Task GetSetupStatus_EmptyAndInitializedStates()
    {
        using IdentityTestFixture fixture = new();
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder => { });
        SystemSetupService service = new(
            fixture.UserManager,
            fixture.RoleManager,
            loggerFactory.CreateLogger<SystemSetupService>());

        var empty = await service.GetSetupStatusAsync(
            CancellationToken.None);
        await fixture.CreateUserAsync("existing@example.com");
        var initialized = await service.GetSetupStatusAsync(
            CancellationToken.None);

        Assert.True(empty.RequiresSetup);
        Assert.False(empty.IsInitialized);
        Assert.False(initialized.RequiresSetup);
        Assert.True(initialized.IsInitialized);
    }

    [Fact]
    public async Task InitializeSetup_CreatesAdministrator()
    {
        using IdentityTestFixture fixture = new();
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder => { });
        SystemSetupService service = new(
            fixture.UserManager,
            fixture.RoleManager,
            loggerFactory.CreateLogger<SystemSetupService>());

        IdentityResultResponse result =
            await service.InitializeSetupAsync(
                "admin@example.com", "Password1!",
                CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(await fixture.RoleManager
            .RoleExistsAsync("Administrator"));
        Assert.NotNull(await fixture.UserManager
            .FindByEmailAsync("admin@example.com"));
    }

    [Fact]
    public async Task InitializeSetup_ExistingUser_ReturnsFailure()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("existing@example.com");
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder => { });
        SystemSetupService service = new(
            fixture.UserManager,
            fixture.RoleManager,
            loggerFactory.CreateLogger<SystemSetupService>());

        IdentityResultResponse result =
            await service.InitializeSetupAsync(
                "admin@example.com", "Password1!",
                CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task InitializeSetup_ExistingRole_ReusesRole()
    {
        using IdentityTestFixture fixture = new();
        Assert.True((await fixture.RoleManager
            .CreateAsync(new Ampere.Infrastructure.Identity.Models.Role(
                "Administrator"))).Succeeded);
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder => { });
        SystemSetupService service = new(
            fixture.UserManager,
            fixture.RoleManager,
            loggerFactory.CreateLogger<SystemSetupService>());

        IdentityResultResponse result =
            await service.InitializeSetupAsync(
                "admin@example.com", "Password1!",
                CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task InitializeSetup_InvalidPassword_ReturnsFailure()
    {
        using IdentityTestFixture fixture = new();
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder => { });
        SystemSetupService service = new(
            fixture.UserManager,
            fixture.RoleManager,
            loggerFactory.CreateLogger<SystemSetupService>());

        IdentityResultResponse result =
            await service.InitializeSetupAsync(
                "admin@example.com", "x",
                CancellationToken.None);

        Assert.False(result.Succeeded);
    }
}
