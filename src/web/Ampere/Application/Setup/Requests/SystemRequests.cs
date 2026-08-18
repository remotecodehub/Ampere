namespace Ampere.Application.Setup.Requests;

/// <summary>Represents the first-time setup administrator credentials.</summary>
/// <param name="Email">The administrator email address.</param>
/// <param name="Password">The administrator password.</param>
public sealed record InitializeSetupRequest(string Email, string Password);

