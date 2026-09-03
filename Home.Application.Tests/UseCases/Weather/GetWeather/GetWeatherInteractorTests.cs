using FluentAssertions;
using Home.Application.Services.Weather;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Weather.GetWeather;
using Home.WebApi.Presenters.Weather.GetWeather;
using Home.WebApi.UseCases.Weather.GetWeather;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace Home.Application.Tests.UseCases.Weather.GetWeather;

/// <summary>
/// The dashboard's weather. The only read with no database behind it, and the only one whose
/// three answers are all correct — no location set, provider unreachable, and an actual forecast.
/// </summary>
public class GetWeatherInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetWeatherPresenter m_Presenter = new(Mapper);
    private readonly Mock<IWeatherService> m_WeatherService = new();

    #endregion Fields

    #region Methods

    private static WeatherSnapshot BuildSnapshot()
        => new(
            WeatherCondition.Showers,
            TemperatureCelsius: 17.5,
            ApparentTemperatureCelsius: 15.0,
            RelativeHumidityPercentage: 72,
            PrecipitationMillimetres: 1.4,
            IsDaytime: true,
            RetrievedUTC: new DateTime(2026, 8, 12, 9, 30, 0),
            Forecast:
            [
                new WeatherDayForecast(new DateTime(2026, 8, 12), WeatherCondition.Showers, 19, 11, 80),
                new WeatherDayForecast(new DateTime(2026, 8, 13), WeatherCondition.Clear, 22, 9, 5)
            ]);

    private Task HandleAsync()
        => new GetWeatherInteractor().HandleAsync(
            new GetWeatherInputPort(),
            this.m_Presenter,
            this.Services().With(this.m_WeatherService.Object).Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_AsksTheProviderForTheSignedInHouseholdsOwnCoordinates()
    {
        this.Ours.Latitude = -37.8136;
        this.Ours.Longitude = 144.9631;

        _ = this.m_WeatherService
            .Setup(w => w.GetForecastAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSnapshot());

        await this.HandleAsync();

        this.m_WeatherService.Verify(
            w => w.GetForecastAsync(-37.8136, 144.9631, It.IsAny<CancellationToken>()),
            Times.Once,
            "the coordinates come off the caller's own household row, never off the request");
    }

    [Fact]
    public async Task HandleAsync_WhenTheForecastComesBack_DescribesItAndNamesAnIconForEveryDay()
    {
        this.Ours.Latitude = -37.8136;
        this.Ours.Longitude = 144.9631;

        _ = this.m_WeatherService
            .Setup(w => w.GetForecastAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSnapshot());

        await this.HandleAsync();

        var _Response = Ok<GetWeatherApiResponse>(this.m_Presenter);

        _ = _Response.HasLocation.Should().BeTrue();
        _ = _Response.Current.Condition.Should().Be("Showers");
        _ = _Response.Current.IconName.Should().Be("weather-showers");
        _ = _Response.Current.TemperatureCelsius.Should().Be(17.5);
        _ = _Response.Forecast.Select(d => d.Condition).Should().Equal(["Showers", "Clear"]);
        _ = _Response.Forecast.Select(d => d.IconName).Should().Equal(
            ["weather-showers", "weather-sun"],
            "a day in the forecast is always drawn as daytime, whatever the hour it is read at");
    }

    [Fact]
    public async Task HandleAsync_WhenTheHouseholdHasNotSaidWhereItIs_SaysSoRatherThanGuessing()
    {
        this.Ours.Latitude = null;
        this.Ours.Longitude = null;

        await this.HandleAsync();

        _ = Ok<GetWeatherApiResponse>(this.m_Presenter).HasLocation.Should().BeFalse();

        this.m_WeatherService.Verify(
            w => w.GetForecastAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTheProviderCannotBeReached_AnswersServiceUnavailableRatherThanFailing()
    {
        this.Ours.Latitude = -37.8136;
        this.Ours.Longitude = 144.9631;

        _ = this.m_WeatherService
            .Setup(w => w.GetForecastAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherSnapshot?)null);

        await this.HandleAsync();

        _ = this.m_Presenter.Result
            .Should().BeOfType<ObjectResult>().Which
            .StatusCode.Should().Be(
                (int)HttpStatusCode.ServiceUnavailable,
                "a dependency being down is something the caller can retry, which is what separates it from a five hundred");
    }

    #endregion Methods

}
