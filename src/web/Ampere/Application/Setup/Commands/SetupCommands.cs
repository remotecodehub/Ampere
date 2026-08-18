using Mediator.Net.Contracts;

namespace Ampere.Application.Setup.Commands;


/// <summary>Requests creation of the initial administrator account.</summary>
/// <param name="Email">The administrator email address.</param>
/// <param name="Password">The administrator password.</param>
public sealed record InitializeSetupCommand(string Email, string Password) : IRequest;