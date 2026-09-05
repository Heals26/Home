using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightSchedules.UpdateLightSchedule;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.LightSchedules.UpdateLightSchedule;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.LightSchedules.UpdateLightSchedule;

/// <summary>
/// Editing a schedule. Moving the time earlier in the day would otherwise make it fire again
/// straight away, so that one change also stamps the last-run marker and lets the normal due
/// check decide.
/// </summary>
public class UpdateLightScheduleInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateLightSchedulePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static LightSchedule BuildSchedule(long lightScheduleID, Household household, string name, TimeSpan timeOfDay, bool isEnabled = true)
        => new()
        {
            DaysOfWeek = 127,
            IsEnabled = isEnabled,
            LightScheduleID = lightScheduleID,
            Name = name,
            Scene = new LightScene()
            {
                Household = household,
                LightSceneID = lightScheduleID + 1000,
                Name = $"Scene for {name}",
                Sequence = 1,
                States = []
            },
            TimeOfDay = timeOfDay,
            Trigger = LightScheduleTrigger.Time
        };

    private Task HandleAsync(
        long lightScheduleID,
        PropertyChangeTracker<int> daysOfWeek = default,
        PropertyChangeTracker<bool> isEnabled = default,
        PropertyChangeTracker<string> name = default,
        PropertyChangeTracker<TimeSpan> timeOfDay = default)
        => new UpdateLightScheduleInteractor().HandleAsync(
            new UpdateLightScheduleInputPort(lightScheduleID, name, isEnabled, timeOfDay, daysOfWeek),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RenamesTheScheduleAndSavesIt()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, "Lights down", new TimeSpan(21, 0, 0)));

        await this.HandleAsync(150, name: new("  Bedtime  "));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<LightSchedule>().Single().Name.Should().Be("Bedtime");
    }

    [Fact]
    public async Task HandleAsync_CanTurnAScheduleOffWithoutDeletingIt()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, "Lights down", new TimeSpan(21, 0, 0)));

        await this.HandleAsync(150, isEnabled: new(false));

        _ = this.Stored<LightSchedule>().Single().IsEnabled.Should().BeFalse();
        _ = this.Stored<LightSchedule>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_MovingTheTimeStampsTheLastRunSoItDoesNotFireImmediately()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, "Lights down", new TimeSpan(21, 0, 0)));

        await this.HandleAsync(150, timeOfDay: new(new TimeSpan(6, 30, 0)));

        var _Stored = this.Stored<LightSchedule>().Single();

        _ = _Stored.TimeOfDay.Should().Be(new TimeSpan(6, 30, 0));
        _ = _Stored.LastRunUTC.Should().Be(
            TestServiceFactory.DefaultNow.UtcDateTime,
            "moving a time earlier would otherwise make the schedule fire again the moment it is saved");
    }

    [Fact]
    public async Task HandleAsync_SavingTheSameTimeLeavesTheLastRunMarkerAlone()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, "Lights down", new TimeSpan(21, 0, 0)));

        await this.HandleAsync(150, timeOfDay: new(new TimeSpan(21, 0, 0)));

        _ = this.Stored<LightSchedule>().Single().LastRunUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_CanChangeWhichDaysItRuns()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, "Lights down", new TimeSpan(21, 0, 0)));

        await this.HandleAsync(150, daysOfWeek: new(62));

        _ = this.Stored<LightSchedule>().Single().DaysOfWeek.Should().Be(62);
    }

    [Fact]
    public async Task HandleAsync_WhenTheScheduleBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(BuildSchedule(950, this.Theirs, "Theirs", new TimeSpan(7, 0, 0)));

        await this.HandleAsync(950, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<LightSchedule>().Single().Name.Should().Be("Theirs");
    }

    #endregion Methods

}
