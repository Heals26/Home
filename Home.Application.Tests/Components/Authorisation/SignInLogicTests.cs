using FluentAssertions;
using Home.WebUI.Infrastructure.Security;
using Home.WebUI.Infrastructure.Values;

namespace Home.Application.Tests.Components.Authorisation;

/// <summary>
/// What the sign-in page says when signing in did not work. All three outcomes used to produce
/// "that username and password didn't match", including an API that was not running and an
/// installation whose own client credentials were wrong.
/// </summary>
public class SignInLogicTests
{

    #region Methods

    [Fact]
    public void DescribeFailure_WhenTheCredentialsWereRefused_AsksThemToTryAgain()
        => SignInLogic.DescribeFailure(TokenRefreshOutcome.Rejected).Should()
            .Contain("username and password").And.Contain("Try again");

    [Fact]
    public void DescribeFailure_WhenThisInstallationWasRefused_DoesNotBlameThePassword()
    {
        var _Message = SignInLogic.DescribeFailure(TokenRefreshOutcome.ClientRejected);

        _ = _Message.Should().Contain(
            "set up correctly",
            "nothing the person types will fix a client credential that does not match the server");
        _ = _Message.Should().NotContain("Try again");
    }

    [Fact]
    public void DescribeFailure_WhenTheApiCouldNotBeReached_SaysSoRatherThanBlamingTheCredentials()
    {
        var _Message = SignInLogic.DescribeFailure(TokenRefreshOutcome.Unavailable);

        _ = _Message.Should().Contain("reach the server");
        _ = _Message.Should().NotContain("username and password");
    }

    [Fact]
    public void DescribeFailure_SaysSomethingDifferentForEveryOutcome()
    {
        string[] _Messages =
        [
            SignInLogic.DescribeFailure(TokenRefreshOutcome.Rejected),
            SignInLogic.DescribeFailure(TokenRefreshOutcome.ClientRejected),
            SignInLogic.DescribeFailure(TokenRefreshOutcome.Unavailable)
        ];

        _ = _Messages.Should().OnlyHaveUniqueItems("collapsing them is the fault this exists to fix");
    }

    #endregion Methods

}
