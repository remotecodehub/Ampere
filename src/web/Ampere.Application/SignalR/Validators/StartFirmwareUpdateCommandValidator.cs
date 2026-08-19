using Ampere.Application.SignalR.Commands;
using FluentValidation;

namespace Ampere.Application.SignalR.Validators;

/// <summary>Validates firmware update commands.</summary>
public sealed class StartFirmwareUpdateCommandValidator
    : AbstractValidator<StartFirmwareUpdateCommand>
{
    /// <summary>Initializes validation rules.</summary>
    public StartFirmwareUpdateCommandValidator()
    {
        RuleFor(command => command.NodeId)
            .NotEmpty();
        RuleFor(command => command.Version)
            .NotEmpty();
    }
}
