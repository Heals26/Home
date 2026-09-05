using FluentAssertions;
using Home.WebUI.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace Home.Application.Tests.Infrastructure.Configuration;

/// <summary>
/// The startup check on the web app's settings. It exists because every one of these used to be
/// read with a null-forgiving operator at the moment of use, so a missing one reached the API as
/// an empty credential and came back as a 401 that the sign-in page reported as a wrong password.
/// </summary>
public class WebUIRequiredConfigurationTests
{

    #region Methods

    private static IConfiguration BuildConfiguration(params (string Key, string? Value)[] overrides)
    {
        var _Values = new Dictionary<string, string?>()
        {
            ["apiBaseUrl"] = "http://localhost:57175",
            ["OAuth:AccessToken:AccessToken"] = "an-access-token",
            ["OAuth:AccessToken:ClientID"] = "1",
            ["OAuth:AccessToken:ClientSecret"] = "a-client-secret",
            ["OAuth:AccessToken:GrantType"] = "password",
            ["OAuth:AccessToken:Scope"] = "WebApp"
        };

        foreach (var (_Key, _Value) in overrides)
            _Values[_Key] = _Value;

        return new ConfigurationBuilder().AddInMemoryCollection(_Values).Build();
    }

    [Fact]
    public void Inspect_WhenEverythingIsSet_FindsNothingWrong()
        => RequiredConfiguration.Inspect(BuildConfiguration()).Should().BeEmpty();

    [Theory]
    [InlineData("apiBaseUrl")]
    [InlineData("OAuth:AccessToken:AccessToken")]
    [InlineData("OAuth:AccessToken:ClientID")]
    [InlineData("OAuth:AccessToken:ClientSecret")]
    [InlineData("OAuth:AccessToken:GrantType")]
    [InlineData("OAuth:AccessToken:Scope")]
    public void Inspect_WhenOneSettingIsMissing_NamesThatSettingAndHowToSetIt(string key)
    {
        var _Problems = RequiredConfiguration.Inspect(BuildConfiguration((key, null)));

        _ = _Problems.Should().ContainSingle().Which.Key.Should().Be(key);
        _ = _Problems.Single().Fix.Should().Contain("dotnet user-secrets set").And.Contain(key);
    }

    [Fact]
    public void Inspect_ReportsEverySettingAtOnceRatherThanTheFirst()
    {
        var _Problems = RequiredConfiguration.Inspect(BuildConfiguration(
            ("apiBaseUrl", null),
            ("OAuth:AccessToken:AccessToken", null),
            ("OAuth:AccessToken:ClientSecret", null)));

        _ = _Problems.Select(p => p.Key).Should().BeEquivalentTo(
            ["apiBaseUrl", "OAuth:AccessToken:AccessToken", "OAuth:AccessToken:ClientSecret"],
            "a fresh install should be told all of what is wrong in one go");
    }

    [Theory]
    [InlineData("http://localhost:57175/api")]
    [InlineData("http://localhost:57175/api/")]
    [InlineData("HTTP://LOCALHOST:57175/API/")]
    public void Inspect_WhenTheBaseUrlAlreadyEndsInApi_CatchesTheDuplicatedSegment(string apiBaseUrl)
    {
        var _Problems = RequiredConfiguration.Inspect(BuildConfiguration(("apiBaseUrl", apiBaseUrl)));

        _ = _Problems.Should().ContainSingle().Which.Problem.Should().Contain(
            "api/api",
            "every route is already prefixed with api, so this 404s on everything");
    }

    [Fact]
    public void Inspect_WhenTheBaseUrlIsNotAbsolute_SaysSo()
        => RequiredConfiguration.Inspect(BuildConfiguration(("apiBaseUrl", "localhost:57175")))
            .Should().ContainSingle().Which.Key.Should().Be("apiBaseUrl");

    [Fact]
    public void Inspect_AllowsABaseUrlHostedUnderAPathThatIsNotApi()
        => RequiredConfiguration.Inspect(BuildConfiguration(("apiBaseUrl", "https://home.example.test/backend")))
            .Should().BeEmpty("a reverse proxy may legitimately mount the API under a path");

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("not-a-number")]
    public void Inspect_WhenTheClientIDIsNotAPositiveNumber_SaysSo(string clientID)
        => RequiredConfiguration.Inspect(BuildConfiguration(("OAuth:AccessToken:ClientID", clientID)))
            .Should().ContainSingle().Which.Key.Should().Be("OAuth:AccessToken:ClientID");

    [Fact]
    public void Validate_WhenEverythingIsSet_DoesNotThrow()
    {
        var _Validate = () => RequiredConfiguration.Validate(BuildConfiguration());

        _ = _Validate.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenSettingsAreMissing_ThrowsNamingEachOfThem()
    {
        var _Validate = () => RequiredConfiguration.Validate(BuildConfiguration(
            ("apiBaseUrl", null),
            ("OAuth:AccessToken:ClientSecret", null)));

        _ = _Validate.Should().Throw<InvalidOperationException>()
            .Which.Message.Should()
                .Contain("apiBaseUrl").And
                .Contain("OAuth:AccessToken:ClientSecret").And
                .Contain("README.md");
    }

    #endregion Methods

}
