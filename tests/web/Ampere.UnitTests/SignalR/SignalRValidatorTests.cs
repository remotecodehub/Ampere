using Ampere.Application.SignalR.Commands;
using Ampere.Application.SignalR.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Ampere.UnitTests.SignalR;

/// <summary>Tests SignalR command validation.</summary>
public sealed class SignalRValidatorTests
{
    [Fact]
    public void StartDiscovery_RequiresValidWindow()
    {
        StartDiscoveryCommandValidator validator = new();

        TestValidationResult<StartDiscoveryCommand> result =
            validator.TestValidate(
                new StartDiscoveryCommand("", 0));

        result.ShouldHaveValidationErrorFor(
            command => command.HouseId);
        result.ShouldHaveValidationErrorFor(
            command => command.WindowSeconds);
    }

    [Fact]
    public void StartDiscovery_AcceptsValidRequest()
    {
        StartDiscoveryCommandValidator validator = new();

        TestValidationResult<StartDiscoveryCommand> result =
            validator.TestValidate(
                new StartDiscoveryCommand("house", 60));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SetRelay_RequiresEndpoint()
    {
        SetRelayCommandValidator validator = new();

        TestValidationResult<SetRelayCommand> result =
            validator.TestValidate(
                new SetRelayCommand("", true));

        result.ShouldHaveValidationErrorFor(
            command => command.EndpointId);
    }

    [Fact]
    public void FirmwareUpdate_RequiresNodeAndVersion()
    {
        StartFirmwareUpdateCommandValidator validator = new();

        TestValidationResult<StartFirmwareUpdateCommand> result =
            validator.TestValidate(
                new StartFirmwareUpdateCommand("", ""));

        result.ShouldHaveValidationErrorFor(
            command => command.NodeId);
        result.ShouldHaveValidationErrorFor(
            command => command.Version);
    }
}
