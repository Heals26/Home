using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightSchedules.GetLightSchedules;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.LightSchedules.GetLightSchedules;
using Home.WebApi.UseCases.LightSchedules.GetLightSchedules;

namespace Home.Application.Tests.UseCases.LightSchedules.GetLightSchedules;

/// <summary>
/// The lights that come on by themselves. A schedule reaches its household through the scene it
/// applies, and the presenter names that scene on every row — the same pairing of a scoping
/// navigation and a displayed one that broke the activity card.
/// </summary>
public class GetLightSchedulesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetLightSchedulesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static LightScene BuildScene(long lightSceneID, Household household, string name)
        => new()
        {
            Household = household,
            LightSceneID = lightSceneID,
            Name = name,
            Sequence = 1
        };

    private static LightSchedule BuildSchedule(long lightScheduleID, LightScene scene, string name, TimeSpan timeOfDay, LightScheduleTrigger trigger = LightScheduleTrigger.Time)
        => new()
        {
            DaysOfWeek = 127,
            IsEnabled = true,
            LightScheduleID = lightScheduleID,
            Name = name,
            Scene = scene,
            TimeOfDay = timeOfDay,
            Trigger = trigger
        };

    private Task HandleAsync()
        => new GetLightSchedulesInteractor().HandleAsync(
            new GetLightSchedulesInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_NamesTheSceneEachScheduleApplies()
    {
        var _Scene = BuildScene(120, this.Ours, "Bedtime");

        _ = this.Database.Seed(BuildSchedule(150, _Scene, "Lights down", new TimeSpan(21, 0, 0)));

        await this.HandleAsync();

        var _Schedule = Ok<GetLightSchedulesApiResponse>(this.m_Presenter).Schedules.Single();

        _ = _Schedule.LightSceneID.Should().Be(120);
        _ = _Schedule.SceneName.Should().Be(
            "Bedtime",
            "the presenter reads the scene on every row, so the query has to load it");
        _ = _Schedule.Name.Should().Be("Lights down");
        _ = _Schedule.TimeOfDay.Should().Be(new TimeSpan(21, 0, 0));
        _ = _Schedule.Trigger.Should().Be(LightScheduleTrigger.Time);
    }

    [Fact]
    public async Task HandleAsync_ReadsSchedulesInTimeOrderThroughTheDay()
    {
        var _Scene = BuildScene(120, this.Ours, "Bedtime");

        _ = this.Database.Seed(
            BuildSchedule(151, _Scene, "Lights down", new TimeSpan(21, 0, 0)),
            BuildSchedule(150, _Scene, "Morning", new TimeSpan(6, 30, 0)),
            BuildSchedule(152, _Scene, "Dusk", new TimeSpan(18, 0, 0), LightScheduleTrigger.Sunset));

        await this.HandleAsync();

        _ = Ok<GetLightSchedulesApiResponse>(this.m_Presenter).Schedules
            .Select(s => s.Name).Should().Equal(["Morning", "Dusk", "Lights down"]);
    }

    [Fact]
    public async Task HandleAsync_ReturnsOnlyTheSchedulesBehindOurOwnScenes()
    {
        _ = this.Database.Seed(
            BuildSchedule(150, BuildScene(120, this.Ours, "Bedtime"), "Lights down", new TimeSpan(21, 0, 0)),
            BuildSchedule(950, BuildScene(920, this.Theirs, "Their look"), "Their timer", new TimeSpan(7, 0, 0)));

        await this.HandleAsync();

        _ = Ok<GetLightSchedulesApiResponse>(this.m_Presenter).Schedules
            .Select(s => s.Name).Should().Equal(["Lights down"]);
    }

    #endregion Methods

}
