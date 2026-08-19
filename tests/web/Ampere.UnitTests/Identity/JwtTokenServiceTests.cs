using System.Security.Claims;
using Ampere.Application.Identity.Abstractions;
using Ampere.Infrastructure.Identity.Options;
using Ampere.Infrastructure.Identity.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace Ampere.UnitTests.Identity;

/// <summary>Tests JWT token creation and validation.</summary>
public sealed class JwtTokenServiceTests
{
    private static JwtTokenService Create(
        RevokedTokenStore? store = null)
    {
        JwtOptions options = new()
        {
            Key = "01234567890123456789012345678901",
            Issuer = "Ampere.Tests",
            Audience = "Ampere.Tests"
        };

        return new JwtTokenService(
            Options.Create(options),
            store ?? new RevokedTokenStore());
    }

    [Fact]
    public void CreateTokens_CreatesAccessAndRefreshTokens()
    {
        JwtTokenService service = Create();

        var result = service.CreateTokens(
            "user-1",
            "user@example.com",
            ["User"],
            [new Claim("permission", "read")]);

        Assert.Equal("Bearer", result.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(
            result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(
            result.RefreshToken));
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsPrincipal()
    {
        JwtTokenService service = Create();
        var result = service.CreateTokens(
            "user-1",
            "user@example.com",
            [],
            []);

        ClaimsPrincipal? principal = service.ValidateToken(
            result.AccessToken);

        Assert.NotNull(principal);
        Assert.Equal(
            "user-1",
            principal.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub"));
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        JwtTokenService service = Create();

        ClaimsPrincipal? result = service.ValidateToken(
            "invalid-token");

        Assert.Null(result);
    }

    [Fact]
    public void ValidateToken_RevokedToken_ReturnsNull()
    {
        RevokedTokenStore store = new();
        JwtTokenService service = Create(store);
        var tokens = service.CreateTokens(
            "user-1",
            "user@example.com",
            [],
            []);
        string tokenId = service.GetTokenId(
            tokens.AccessToken)!;
        DateTimeOffset expiration = service.GetExpiration(
            tokens.AccessToken)!.Value;

        store.Revoke(tokenId, expiration);

        ClaimsPrincipal? result = service.ValidateToken(
            tokens.AccessToken);

        Assert.Null(result);
    }

    [Fact]
    public void GetTokenId_InvalidToken_ReturnsNull()
    {
        JwtTokenService service = Create();

        Assert.Null(service.GetTokenId("invalid"));
    }

    [Fact]
    public void GetExpiration_InvalidToken_ReturnsNull()
    {
        JwtTokenService service = Create();

        Assert.Null(service.GetExpiration("invalid"));
    }

    [Fact]
    public void CreateTokens_ShortKey_Throws()
    {
        JwtOptions options = new()
        {
            Key = "short",
            Issuer = "Ampere.Tests",
            Audience = "Ampere.Tests"
        };
        JwtTokenService service = new(
            Options.Create(options),
            new RevokedTokenStore());

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateTokens(
                "user-1",
                "user@example.com",
                [],
                []));
    }
}
