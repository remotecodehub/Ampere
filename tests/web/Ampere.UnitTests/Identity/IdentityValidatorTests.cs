using Ampere.Application.Identity.Commands;
using Ampere.Application.Identity.Responses;
using Ampere.Application.Identity.Validators;
using Xunit;

namespace Ampere.UnitTests.Identity;

/// <summary>Tests Identity command validation rules.</summary>
public sealed class IdentityValidatorTests
{
    [Fact]
    public async Task RegisterValidator_ValidAndInvalidInputs()
    {
        FakeIdentityService service = new();
        RegisterCommandValidator validator = new(service);

        FluentValidation.Results.ValidationResult valid =
            await validator.ValidateAsync(new RegisterCommand(
                "user@example.com", "Password1!"));
        FluentValidation.Results.ValidationResult invalid =
            await validator.ValidateAsync(new RegisterCommand(
                "user@example.com", "short"));

        Assert.True(valid.IsValid);
        Assert.False(invalid.IsValid);
    }

    [Fact]
    public async Task LoginValidator_CoversCredentialsAnd2FaRules()
    {
        LoginCommandValidator validator = new();

        Assert.True((await validator.ValidateAsync(
            new LoginCommand(
                "user@example.com", "Password1!"))).IsValid);
        Assert.False((await validator.ValidateAsync(
            new LoginCommand("", "short", "123456", "code")))
            .IsValid);
        Assert.False((await validator.ValidateAsync(
            new LoginCommand(
                "user@example.com", "Password1!",
                "123456", "code"))).IsValid);
    }

    [Fact]
    public async Task SimpleTokenValidators_RejectEmptyValues()
    {
        Assert.False((await new RefreshTokenCommandValidator()
            .ValidateAsync(new RefreshTokenCommand(""))).IsValid);
        Assert.True((await new RefreshTokenCommandValidator()
            .ValidateAsync(new RefreshTokenCommand("token"))).IsValid);
        Assert.False((await new RevokeTokenCommandValidator()
            .ValidateAsync(new RevokeTokenCommand(""))).IsValid);
        Assert.True((await new RevokeTokenCommandValidator()
            .ValidateAsync(new RevokeTokenCommand("token"))).IsValid);
    }

    [Fact]
    public async Task ConfirmEmailValidator_RequiresUserAndCode()
    {
        ConfirmEmailCommandValidator validator = new();

        Assert.True((await validator.ValidateAsync(
            new ConfirmEmailCommand("user", "code"))).IsValid);
        Assert.False((await validator.ValidateAsync(
            new ConfirmEmailCommand("", ""))).IsValid);
    }

    [Fact]
    public async Task EmailValidators_RejectInvalidAddresses()
    {
        ResendConfirmationEmailCommandValidator resend = new();
        ForgotPasswordCommandValidator forgot = new();

        Assert.True((await resend.ValidateAsync(
            new ResendConfirmationEmailCommand(
                "user@example.com"))).IsValid);
        Assert.False((await resend.ValidateAsync(
            new ResendConfirmationEmailCommand("bad"))).IsValid);
        Assert.True((await forgot.ValidateAsync(
            new ForgotPasswordCommand(
                "user@example.com"))).IsValid);
        Assert.False((await forgot.ValidateAsync(
            new ForgotPasswordCommand("bad"))).IsValid);
    }

    [Fact]
    public async Task ResetPasswordValidator_ValidatesAllFields()
    {
        ResetPasswordCommandValidator validator = new();

        Assert.True((await validator.ValidateAsync(
            new ResetPasswordCommand(
                "user@example.com", "code", "Password1!")))
            .IsValid);
        Assert.False((await validator.ValidateAsync(
            new ResetPasswordCommand(
                "bad", "", "short"))).IsValid);
    }

    [Fact]
    public async Task UpdateIdentityValidator_OptionalFieldsHaveRules()
    {
        UpdateIdentityInfoCommandValidator validator = new();

        Assert.True((await validator.ValidateAsync(
            new UpdateIdentityInfoCommand(
                "user", null, null, "Password1!"))).IsValid);
        Assert.False((await validator.ValidateAsync(
            new UpdateIdentityInfoCommand(
                "", "bad", "short", ""))).IsValid);
    }

    private sealed class FakeIdentityService :
        Ampere.Application.Identity.Abstractions.IIdentityService
    {
        public Task<IdentityResultResponse> RegisterAsync(
            string email, string password,
            CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResultResponse.Success());

        public Task<bool> EmailExistsAsync(
            string email, CancellationToken cancellationToken) =>
            Task.FromResult(email == "existing@example.com");

        public Task<TokenResponse?> LoginAsync(
            string email, string password, string? twoFactorCode,
            string? twoFactorRecoveryCode,
            CancellationToken cancellationToken) =>
            Task.FromResult<TokenResponse?>(null);

        public Task<TokenResponse?> RefreshAsync(
            string refreshToken,
            CancellationToken cancellationToken) =>
            Task.FromResult<TokenResponse?>(null);

        public Task<bool> RevokeAsync(
            string accessToken,
            CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task<bool> ConfirmEmailAsync(
            string userId, string code, string? changedEmail,
            CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task<IdentityResultResponse>
            ResendConfirmationEmailAsync(
                string email,
                CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResultResponse.Success());

        public Task<IdentityResultResponse> ForgotPasswordAsync(
            string email, CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResultResponse.Success());

        public Task<IdentityResultResponse> ResetPasswordAsync(
            string email, string resetCode, string newPassword,
            CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResultResponse.Success());

        public Task<IdentityInfoResponse?> GetInfoAsync(
            string userId, CancellationToken cancellationToken) =>
            Task.FromResult<IdentityInfoResponse?>(null);

        public Task<IdentityResultResponse> UpdateInfoAsync(
            string userId, string? newEmail, string? newPassword,
            string oldPassword, CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResultResponse.Success());

        public Task<TwoFactorResponse?> ConfigureTwoFactorAsync(
            string userId, bool? enable, string? twoFactorCode,
            bool resetRecoveryCodes, bool resetSharedKey,
            bool forgetMachine, CancellationToken cancellationToken) =>
            Task.FromResult<TwoFactorResponse?>(null);
    }
}
