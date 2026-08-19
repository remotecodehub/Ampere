using Ampere.Application.Identity.Responses;
using Ampere.Infrastructure.Identity.Options;
using Ampere.Infrastructure.Identity.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Ampere.UnitTests.Identity;

/// <summary>Tests supporting Identity infrastructure branches.</summary>
public sealed class IdentityInfrastructureBranchTests
{
    [Fact]
    public async Task LoggingEmailSender_LogsBothMessageKinds()
    {
        using ILoggerFactory factory = LoggerFactory.Create(
            builder => builder.AddDebug());
        ILogger<LoggingIdentityEmailSender> logger =
            factory.CreateLogger<LoggingIdentityEmailSender>();
        LoggingIdentityEmailSender sender = new(logger);

        await sender.SendConfirmationAsync(
            "user@example.com", "/confirm",
            CancellationToken.None);
        await sender.SendPasswordResetAsync(
            "user@example.com", "/reset",
            CancellationToken.None);
    }

    [Fact]
    public void JwtOptions_DefaultsAreConfigured()
    {
        JwtOptions options =
            new()
            {
                Key = "01234567890123456789012345678901"
            };

        Assert.Equal("Ampere", options.Issuer);
        Assert.Equal("Ampere", options.Audience);
        Assert.Equal(TimeSpan.FromMinutes(15),
            options.AccessTokenLifetime);
        Assert.Equal(TimeSpan.FromDays(14),
            options.RefreshTokenLifetime);
    }
}
