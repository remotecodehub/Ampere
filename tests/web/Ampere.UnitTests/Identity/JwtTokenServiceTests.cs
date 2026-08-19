using System.Security.Claims;
using Ampere.Infrastructure.Identity.Options;
using Ampere.Infrastructure.Identity.Services;
using Microsoft.Extensions.Options;

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
            "user-1", "user@example.com", ["User"],
            [new Claim("permission", "read")]);

        Assert.Equal("Bearer", result.TokenType);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsPrincipal()
    {
        JwtTokenService service = Create();
        var tokens = service.CreateTokens(
            "user-1", "user@example.com", [], []);

        ClaimsPrincipal? principal = service.ValidateToken(
            tokens.AccessToken);

        Assert.NotNull(principal);
        Assert.Equal("user-1", principal.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub"));
    }

    [Fact]
    public void ValidateToken_EmptyToken_ReturnsNull()
    {
        JwtTokenService service = Create();

        Assert.Null(service.ValidateToken(string.Empty));
        Assert.Null(service.ValidateToken(" "));
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        JwtTokenService service = Create();

        Assert.Null(service.ValidateToken("invalid-token"));
    }

    [Fact]
    public void ValidateToken_RevokedToken_ReturnsNull()
    {
        RevokedTokenStore store = new();
        JwtTokenService service = Create(store);
        var tokens = service.CreateTokens(
            "user-1", "user@example.com", [], []);
        string tokenId = service.GetTokenId(
            tokens.AccessToken)!;
        DateTimeOffset expiration = service.GetExpiration(
            tokens.AccessToken)!.Value;
        store.Revoke(tokenId, expiration);

        Assert.Null(service.ValidateToken(
            tokens.AccessToken));
    }

    [Fact]
    public void ValidateToken_ExpiredTokenWithoutLifetimeValidation_ReturnsPrincipal()
    {
        JwtOptions options = new()
        {
            Key = "01234567890123456789012345678901",
            Issuer = "Ampere.Tests",
            Audience = "Ampere.Tests",
            AccessTokenLifetime = TimeSpan.FromSeconds(-1)
        };
        JwtTokenService service = new(
            Options.Create(options),
            new RevokedTokenStore());
        var tokens = service.CreateTokens(
            "user-1", "user@example.com", [], []);

        ClaimsPrincipal? principal = service.ValidateToken(
            tokens.AccessToken, false);

        Assert.NotNull(principal);
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
    public void GetExpiration_ValidToken_ReturnsExpiration()
    {
        JwtTokenService service = Create();
        var tokens = service.CreateTokens(
            "user-1", "user@example.com", [], []);

        DateTimeOffset? expiration = service.GetExpiration(
            tokens.AccessToken);

        Assert.NotNull(expiration);
        Assert.True(expiration > DateTimeOffset.UtcNow);
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
                "user-1", "user@example.com", [], []));
    }
}
