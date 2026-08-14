using FluentAssertions;
using Home.Application.Infrastructure.Lights;

namespace Home.Application.Tests.Infrastructure.Lights;

public class SunCalculatorTests
{

    #region Fields

    // Brisbane — the household the app is being built around.
    private const double BrisbaneLatitude = -27.47;
    private const double BrisbaneLongitude = 153.03;
    private static readonly TimeSpan BrisbaneOffset = TimeSpan.FromHours(10);

    #endregion Fields

    #region Methods

    [Fact]
    public void GetSunEventLocalTime_BrisbaneAugustSunriseLandsAroundQuarterPastSix()
    {
        var _Sunrise = SunCalculator.GetSunEventLocalTime(
            new DateOnly(2026, 8, 14), BrisbaneLatitude, BrisbaneLongitude, BrisbaneOffset, sunrise: true);

        _Sunrise.Should().NotBeNull();
        _Sunrise!.Value.Should().BeGreaterThan(new TimeSpan(5, 55, 0)).And.BeLessThan(new TimeSpan(6, 35, 0));
    }

    [Fact]
    public void GetSunEventLocalTime_BrisbaneAugustSunsetLandsAroundHalfPastFive()
    {
        var _Sunset = SunCalculator.GetSunEventLocalTime(
            new DateOnly(2026, 8, 14), BrisbaneLatitude, BrisbaneLongitude, BrisbaneOffset, sunrise: false);

        _Sunset.Should().NotBeNull();
        _Sunset!.Value.Should().BeGreaterThan(new TimeSpan(17, 5, 0)).And.BeLessThan(new TimeSpan(17, 50, 0));
    }

    [Fact]
    public void GetSunEventLocalTime_ReturnsNullThroughPolarNight()
    {
        // Svalbard in mid December — the sun does not rise at all.
        var _Sunrise = SunCalculator.GetSunEventLocalTime(
            new DateOnly(2026, 12, 15), 78.22, 15.63, TimeSpan.FromHours(1), sunrise: true);

        _Sunrise.Should().BeNull();
    }

    [Fact]
    public void GetSunEventLocalTime_SunriseAlwaysPrecedesSunsetOnAnOrdinaryDay()
    {
        var _Sunrise = SunCalculator.GetSunEventLocalTime(
            new DateOnly(2026, 3, 1), BrisbaneLatitude, BrisbaneLongitude, BrisbaneOffset, sunrise: true);
        var _Sunset = SunCalculator.GetSunEventLocalTime(
            new DateOnly(2026, 3, 1), BrisbaneLatitude, BrisbaneLongitude, BrisbaneOffset, sunrise: false);

        _Sunrise.Should().NotBeNull();
        _Sunset.Should().NotBeNull();
        _Sunrise!.Value.Should().BeLessThan(_Sunset!.Value);
    }

    #endregion Methods

}
