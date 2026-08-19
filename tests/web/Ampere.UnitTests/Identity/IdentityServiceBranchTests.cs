using System.Security.Claims;
using Ampere.Infrastructure.Identity.Models;
using Ampere.UnitTests.Common.Fixtures;
using Xunit;

namespace Ampere.UnitTests.Identity;

/// <summary>Exercises less common Identity branches.</summary>
public sealed class IdentityServiceBranchTests
{
    [Fact]
    public async Task LoginAsync_TwoFactorRecoveryCode_Succeeds()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "recovery@example.com");
        await fixture.UserManager.ResetAuthenticatorKeyAsync(user);
        string[]? codes = (string[]?)await fixture.UserManager
            .GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        await fixture.UserManager.SetTwoFactorEnabledAsync(
            user, true);

        string recoveryCode = codes![0];
        var result = await fixture.Service.LoginAsync(
            user.Email!, "Password1!", null, recoveryCode,
            CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task RefreshAsync_MissingUser_ReturnsNull()
    {
        using IdentityTestFixture fixture = new();
        var tokens = fixture.Service
            .CreateTokensForTest("missing-user");

        Assert.Null(await fixture.Service.RefreshAsync(
            tokens.RefreshToken, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_UserWithRole_ReturnsTokens()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "role-claims@example.com");
        Role role = new("Operator");
        Assert.True((await fixture.RoleManager.CreateAsync(role))
            .Succeeded);
        Assert.True((await fixture.RoleManager.AddClaimAsync(
            role,
            new Claim("permission", "read"))).Succeeded);
        Assert.True((await fixture.UserManager.AddToRoleAsync(
            user,
            role.Name!)).Succeeded);

        var result = await fixture.Service.LoginAsync(
            user.Email!, "Password1!", null, null,
            CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateInfoAsync_EmailConflict_Fails()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "owner@example.com");
        await fixture.CreateUserAsync("taken@example.com");

        var result = await fixture.Service.UpdateInfoAsync(
            user.Id, "taken@example.com", null,
            "Password1!", CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpdateInfoAsync_InvalidNewPassword_Fails()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "bad-password@example.com");

        var result = await fixture.Service.UpdateInfoAsync(
            user.Id, null, "x", "Password1!",
            CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ConfigureTwoFactorAsync_ResetKey_ReturnsKey()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "reset-key@example.com");

        var result = await fixture.Service
            .ConfigureTwoFactorAsync(
                user.Id, false, null, false, true, false,
                CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(
            result.SharedKey));
    }

    [Fact]
    public async Task ConfigureTwoFactorAsync_ResetRecoveryCodes_ReturnsCodes()
    {
        using IdentityTestFixture fixture = new();
        User user = await fixture.CreateUserAsync(
            "reset-codes@example.com");

        var result = await fixture.Service
            .ConfigureTwoFactorAsync(
                user.Id, false, null, true, false, false,
                CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.RecoveryCodes);
        Assert.NotEmpty(result.RecoveryCodes!);
    }

    [Fact]
    public async Task EmailExistsAsync_IsCaseSensitiveByStoreRules()
    {
        using IdentityTestFixture fixture = new();
        await fixture.CreateUserAsync("case@example.com");

        Assert.True(await fixture.Service.EmailExistsAsync(
            "case@example.com", CancellationToken.None));
    }
}
