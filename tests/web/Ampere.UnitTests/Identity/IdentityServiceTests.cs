using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Identity.Services;
using Ampere.UnitTests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Ampere.UnitTests.Identity;

/// <summary>Tests the application Identity service.</summary>
public sealed class IdentityServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsSuccess()
    {
        using IdentityTestFixture fixture = new();

        var result = await fixture.Service.RegisterAsync(
            "new@example.com",
            "Password1!",
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(await fixture.DbContext.Users
            .SingleOrDefaultAsync(x =>
                x.Email == "new@example.com"));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUser_ReturnsFailure()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("same@example.com");

        var result = await fixture.Service.RegisterAsync(
            "same@example.com",
            "Password1!",
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("login@example.com");

        var result = await fixture.Service.LoginAsync(
            "login@example.com",
            "Password1!",
            null,
            null,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ReturnsNull()
    {
        using IdentityTestFixture fixture = new();

        var result = await fixture.Service.LoginAsync(
            "missing@example.com",
            "Password1!",
            null,
            null,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("wrong@example.com");

        var result = await fixture.Service.LoginAsync(
            "wrong@example.com",
            "WrongPassword",
            null,
            null,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_ValidRefreshToken_ReturnsNewTokens()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("refresh@example.com");
        var login = await fixture.Service.LoginAsync(
            "refresh@example.com",
            "Password1!",
            null,
            null,
            CancellationToken.None);

        var result = await fixture.Service.RefreshAsync(
            login!.RefreshToken,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(login.RefreshToken,
            result.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_AccessToken_ReturnsNull()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("refresh2@example.com");
        var login = await fixture.Service.LoginAsync(
            "refresh2@example.com",
            "Password1!",
            null,
            null,
            CancellationToken.None);

        var result = await fixture.Service.RefreshAsync(
            login!.AccessToken,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeAsync_ValidToken_ReturnsTrue()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("revoke@example.com");
        var login = await fixture.Service.LoginAsync(
            "revoke@example.com",
            "Password1!",
            null,
            null,
            CancellationToken.None);

        Assert.True(await fixture.Service.RevokeAsync(
            login!.AccessToken,
            CancellationToken.None));
    }

    [Fact]
    public async Task RevokeAsync_InvalidToken_ReturnsFalse()
    {
        using IdentityTestFixture fixture = new();

        Assert.False(await fixture.Service.RevokeAsync(
            "invalid",
            CancellationToken.None));
    }

    [Fact]
    public async Task ConfirmEmailAsync_ValidCode_ConfirmsEmail()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "confirm@example.com");
        UserManager<User> manager =
            fixture.DbContext.GetService<
                UserManager<User>>();
        string code = await manager
            .GenerateEmailConfirmationTokenAsync(user);

        var result = await fixture.Service.ConfirmEmailAsync(
            user.Id,
            code,
            null,
            CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task ConfirmEmailAsync_UnknownUser_ReturnsFalse()
    {
        using IdentityTestFixture fixture = new();

        Assert.False(await fixture.Service.ConfirmEmailAsync(
            "missing",
            "code",
            null,
            CancellationToken.None));
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_ConfirmedUser_IsSuccess()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("confirmed@example.com");

        var result = await fixture.Service
            .ResendConfirmationEmailAsync(
                "confirmed@example.com",
                CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ExistingUser_IsSuccess()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("forgot@example.com");

        var result = await fixture.Service.ForgotPasswordAsync(
            "forgot@example.com",
            CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ForgotPasswordAsync_UnknownUser_IsSuccess()
    {
        using IdentityTestFixture fixture = new();

        var result = await fixture.Service.ForgotPasswordAsync(
            "missing@example.com",
            CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidCode_Succeeds()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "reset@example.com");
        UserManager<User> manager =
            fixture.DbContext.GetService<
                UserManager<User>>();
        string code = await manager
            .GeneratePasswordResetTokenAsync(user);

        var result = await fixture.Service.ResetPasswordAsync(
            user.Email!,
            code,
            "NewPassword1!",
            CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ResetPasswordAsync_UnknownUser_Fails()
    {
        using IdentityTestFixture fixture = new();

        var result = await fixture.Service.ResetPasswordAsync(
            "missing@example.com",
            "code",
            "NewPassword1!",
            CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task GetInfoAsync_ExistingUser_ReturnsInfo()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "info@example.com");

        var result = await fixture.Service.GetInfoAsync(
            user.Id,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.True(result.IsEmailConfirmed);
    }

    [Fact]
    public async Task GetInfoAsync_UnknownUser_ReturnsNull()
    {
        using IdentityTestFixture fixture = new();

        Assert.Null(await fixture.Service.GetInfoAsync(
            "missing",
            CancellationToken.None));
    }

    [Fact]
    public async Task UpdateInfoAsync_WrongPassword_Fails()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "update@example.com");

        var result = await fixture.Service.UpdateInfoAsync(
            user.Id,
            "new@example.com",
            "NewPassword1!",
            "wrong",
            CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpdateInfoAsync_EmailAndPassword_Succeeds()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "update2@example.com");

        var result = await fixture.Service.UpdateInfoAsync(
            user.Id,
            "updated@example.com",
            "NewPassword1!",
            "Password1!",
            CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ConfigureTwoFactorAsync_UnknownUser_ReturnsNull()
    {
        using IdentityTestFixture fixture = new();

        Assert.Null(await fixture.Service.ConfigureTwoFactorAsync(
            "missing",
            null,
            null,
            false,
            false,
            false,
            CancellationToken.None));
    }

    [Fact]
    public async Task ConfigureTwoFactorAsync_Disable_ReturnsConfiguration()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "twofactor@example.com");

        var result = await fixture.Service
            .ConfigureTwoFactorAsync(
                user.Id,
                false,
                null,
                false,
                false,
                false,
                CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.IsTwoFactorEnabled);
    }
}
