using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightSchedules.CreateLightSchedule;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.LightSchedules.CreateLightSchedule;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.LightSchedules.CreateLightSchedule;

/// <summary>
/// Setting lights to come on by themselves. A schedule reaches its household through the scene it
/// applies, so naming another household's scene is what has to miss.
/// </summary>
public class CreateLightScheduleInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateLightSchedulePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static LightScene BuildScene(long lightSceneID, Household household, string name)
        => new()
        {
            Household = household,
            LightSceneID = lightSceneID,
            Name = name,
            Sequence = 1,
            States = []
        };

    private Task HandleAsync(
        long lightSceneID,
        string name = "Lights down",
        LightScheduleTrigger trigger = LightScheduleTrigger.Time,
        TimeSpan timeOfDay = default,
        int offsetMinutes = 0,
        int daysOfWeek = 127)
        => new CreateLightScheduleInteractor().HandleAsync(
            new CreateLightScheduleInputPort(name, lightSceneID, trigger, timeOfDay, offsetMinutes, daysOfWeek),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheScheduleAgainstTheScene()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Bedtime"));

        await this.HandleAsync(120, "  Lights down  ", timeOfDay: new TimeSpan(21, 0, 0), daysOfWeek: 62);

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<LightSchedule>().Single();

        _ = _Stored.Name.Should().Be("Lights down");
        _ = _Stored.TimeOfDay.Should().Be(new TimeSpan(21, 0, 0));
        _ = _Stored.DaysOfWeek.Should().Be(62);
        _ = this.Stored<LightSchedule>().Count(s => s.Scene.LightSceneID == 120).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_StartsAScheduleTurnedOn()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Bedtime"));

        await this.HandleAsync(120);

        _ = this.Stored<LightSchedule>().Single().IsEnabled.Should().BeTrue(
            "nobody sets up a schedule in order to leave it off");
    }

    [Fact]
    public async Task HandleAsync_KeepsTheSunOffsetForASunriseOrSunsetSchedule()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Bedtime"));

        await this.HandleAsync(120, trigger: LightScheduleTrigger.Sunset, offsetMinutes: -30);

        var _Stored = this.Stored<LightSchedule>().Single();

        _ = _Stored.Trigger.Should().Be(LightScheduleTrigger.Sunset);
        _ = _Stored.OffsetMinutes.Should().Be(-30);
    }

    [Fact]
    public async Task HandleAsync_WhenTheSceneBelongsToAnotherHousehold_PresentsNotFoundAndWritesNothing()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Bedtime"), BuildScene(920, this.Theirs, "Theirs"));

        await this.HandleAsync(920);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<LightSchedule>().Should().BeEmpty();
    }

    #endregion Methods

}
