using Ampere.Application.Common.Responses;
using Xunit;

namespace Ampere.UnitTests.Common;

/// <summary>Tests application response factories.</summary>
public sealed class ResponseTests
{
    [Fact]
    public void ResponseFactories_CreateExpectedResults()
    {
        Response<string> success =
            Response.Success("data");
        Response<string> enumerableFailure =
            Response.Failure<string>(
                new[] { "error" });
        Response<string> paramsFailure =
            Response.Failure<string>("error", "second");

        Assert.True(success.Succeeded);
        Assert.Equal("data", success.Data);
        Assert.False(enumerableFailure.Succeeded);
        Assert.Single(enumerableFailure.Errors);
        Assert.False(paramsFailure.Succeeded);
        Assert.Equal(2, paramsFailure.Errors.Count);
    }
}
