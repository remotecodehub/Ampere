using Ampere.Application.SignalR.Commands;
using FluentValidation;

namespace Ampere.Application.SignalR.Validators;

/// <summary>Validates relay commands.</summary>
public sealed class SetRelayCommandValidator
    : AbstractValidator<SetRelayCommand>
{
    /// <summary>Initializes validation rules.</summary>
    public SetRelayCommandValidator()
    {
        RuleFor(command => command.EndpointId)
            .NotEmpty();
    }
}
