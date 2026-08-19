using Ampere.Application.SignalR.Commands;
using FluentValidation;

namespace Ampere.Application.SignalR.Validators;

/// <summary>Validates discovery commands.</summary>
public sealed class StartDiscoveryCommandValidator
    : AbstractValidator<StartDiscoveryCommand>
{
    /// <summary>Initializes validation rules.</summary>
    public StartDiscoveryCommandValidator()
    {
        RuleFor(command => command.HouseId)
            .NotEmpty();
        RuleFor(command => command.WindowSeconds)
            .InclusiveBetween(1, 600);
    }
}
