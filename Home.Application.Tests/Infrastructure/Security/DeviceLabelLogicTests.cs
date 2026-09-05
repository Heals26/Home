using FluentAssertions;
using Home.Application.Infrastructure.Security;

namespace Home.Application.Tests.Infrastructure.Security;

/// <summary>
/// Naming a signed-in device from its User-Agent. Every Chromium browser still claims to be Chrome
/// and Safari, Chrome still claims to be Safari, and an iPhone still claims to be a Mac, so the
/// order these are tested in is the whole substance of the class.
/// </summary>
public class DeviceLabelLogicTests
{

    #region Methods

    [Theory]
    [InlineData(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0 Safari/537.36",
        "Chrome on Windows")]
    [InlineData(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0 Safari/537.36 Edg/128.0",
        "Edge on Windows")]
    [InlineData(
        "Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Safari/604.1",
        "Safari on iPad")]
    [InlineData(
        "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile Safari/604.1",
        "Safari on iPhone")]
    [InlineData(
        "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0 Mobile Safari/537.36",
        "Chrome on Android")]
    [InlineData(
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0 Safari/537.36",
        "Chrome on Mac")]
    [InlineData(
        "Mozilla/5.0 (X11; Linux x86_64; rv:128.0) Gecko/20100101 Firefox/128.0",
        "Firefox on Linux")]
    [InlineData(
        "Mozilla/5.0 (Linux; Android 14; SM-S918B) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/25.0 Chrome/121.0 Mobile Safari/537.36",
        "Samsung Internet on Android")]
    public void Describe_NamesTheBrowserAndTheMachineItIsOn(string userAgent, string expected)
        => DeviceLabelLogic.Describe(userAgent).Should().Be(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Describe_WhenNothingUsefulWasSent_SaysSoRatherThanShowingAnEmptyRow(string? userAgent)
        => DeviceLabelLogic.Describe(userAgent).Should().Be(DeviceLabelLogic.UnknownDevice);

    [Fact]
    public void Describe_WhenOnlyTheMachineIsRecognisable_NamesJustTheMachine()
        => DeviceLabelLogic.Describe("SomeScript/1.0 (Windows NT 10.0)").Should().Be("Windows");

    [Fact]
    public void Describe_WhenNothingIsRecognisable_SaysUnknownRatherThanEchoingTheHeader()
        => DeviceLabelLogic.Describe("curl/8.4.0").Should().Be(DeviceLabelLogic.UnknownDevice);

    [Fact]
    public void Describe_TruncatesAHostileUserAgentRatherThanCarryingItIntoTheColumn()
    {
        var _Label = DeviceLabelLogic.Describe(new string('A', 5000) + " Windows");

        _ = _Label.Length.Should().BeLessThanOrEqualTo(100);
    }

    #endregion Methods

}
