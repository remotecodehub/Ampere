using Ampere.Application.Identity.Responses;
using Ampere.Application.Setup.Commands;
using Ampere.Application.Setup.Queries;
using Ampere.Application.Setup.Requests;
using Ampere.Application.Setup.Responses;
using Ampere.Controllers.v1;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ampere.UnitTests.Presentation;

/// <summary>Tests setup controller dispatch and mapping.</summary>
public sealed class SetupControllerTests
{
    [Fact]
    public async Task GetStatus_DispatchesQuery()
    {
        Mock<IMediator> mediator = new();
        SetupStatusResponse response =
            new(true, false);
        mediator.Setup(item => item.Send(
                It.IsAny<GetSetupStatusQuery>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<SetupStatusResponse>(response));
        SetupController controller = new(mediator.Object);

        IActionResult result = await controller.GetStatus(
            CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task Initialize_Success_ReturnsOk()
    {
        Mock<IMediator> mediator = new();
        IdentityResultResponse response =
            IdentityResultResponse.Success();
        mediator.Setup(item => item.Send(
                It.IsAny<InitializeSetupCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<IdentityResultResponse>(response));
        SetupController controller = new(mediator.Object);

        IActionResult result = await controller.Initialize(
            new InitializeSetupRequest(
                "admin@ampere.local",
                "Password1!"),
            CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
        mediator.Verify(item => item.Send(
                It.Is<InitializeSetupCommand>(command =>
                    command.Email == "admin@ampere.local"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Initialize_Failure_ReturnsConflict()
    {
        Mock<IMediator> mediator = new();
        IdentityResultResponse response =
            IdentityResultResponse.Failure(["already initialized"]);
        mediator.Setup(item => item.Send(
                It.IsAny<InitializeSetupCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<IdentityResultResponse>(response));
        SetupController controller = new(mediator.Object);

        IActionResult result = await controller.Initialize(
            new InitializeSetupRequest(
                "admin@ampere.local",
                "Password1!"),
            CancellationToken.None);

        ConflictObjectResult conflict =
            Assert.IsType<ConflictObjectResult>(result);
        Assert.Same(response, conflict.Value);
    }
}
