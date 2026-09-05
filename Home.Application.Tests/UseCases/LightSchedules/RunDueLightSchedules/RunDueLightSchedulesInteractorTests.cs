using FluentAssertions;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Lights;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightSchedules.RunDueLightSchedules;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Moq;

namespace Home.Application.Tests.UseCases.LightSchedules.RunDueLightSchedules;

/// <summary>
/// The timer tick that fires schedules. It runs for every household at once and deliberately does
/// not use the authorisation service, because there is nobody signed in behind a timer.
/// <para>
/// The due window is open-ended rather than an exact minute, so a tick the runner missed still
/// fires late instead of skipping the day. The clock is fixed at Wednesday 12 Aug 2026, 09:30.
/// </para>
/// </summary>
public class RunDueLightSchedulesInteractorTests : InteractorTest
{

    #region Constants

    private const int EveryDay = 127;

    #endregion Constants

    #region Fields

    private readonly Mock<IRunDueLightSchedulesOutputPort> m_OutputPort = new();
    private readonly Mock<ILightSceneLogic> m_SceneLogic = new();

    #endregion Fields

    #region Methods

    private static LightSchedule BuildSchedule(
        long lightScheduleID,
        Household household,
        TimeSpan timeOfDay,
        bool isEnabled = true,
        int daysOfWeek = EveryDay,
        DateTime? lastRunUTC = null)
        => new()
        {
            DaysOfWeek = daysOfWeek,
            IsEnabled = isEnabled,
            LastRunUTC = lastRunUTC,
            LightScheduleID = lightScheduleID,
            Name = $"Schedule {lightScheduleID}",
            Scene = new LightScene()
            {
                Household = household,
                LightSceneID = lightScheduleID + 1000,
                Name = $"Scene {lightScheduleID}",
                Sequence = 1,
                States = []
            },
            TimeOfDay = timeOfDay,
            Trigger = LightScheduleTrigger.Time
        };

    private Task HandleAsync(LightCommandResult result = LightCommandResult.Applied)
    {
        _ = this.m_SceneLogic
            .Setup(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return new RunDueLightSchedulesInteractor().HandleAsync(
            new RunDueLightSchedulesInputPort(),
            this.m_OutputPort.Object,
            this.Services().With(this.m_SceneLogic.Object).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_FiresAScheduleWhoseTimeHasPassedToday()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, new TimeSpan(6, 0, 0)));

        await this.HandleAsync();

        this.m_SceneLogic.Verify(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()), Times.Once);
        this.m_OutputPort.Verify(o => o.PresentSchedulesRunAsync(1, 0, It.IsAny<CancellationToken>()), Times.Once);
        _ = this.Stored<LightSchedule>().Single().LastRunUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_LeavesAScheduleWhoseTimeHasNotComeYet()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, new TimeSpan(21, 0, 0)));

        await this.HandleAsync();

        this.m_SceneLogic.Verify(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()), Times.Never);
        _ = this.Stored<LightSchedule>().Single().LastRunUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_SkipsAScheduleThatIsTurnedOff()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, new TimeSpan(6, 0, 0), isEnabled: false));

        await this.HandleAsync();

        this.m_SceneLogic.Verify(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SkipsAScheduleThatDoesNotRunToday()
    {
        // The fixed clock is a Wednesday, so a weekend-only schedule has nothing to do.
        const int _SaturdayAndSunday = (1 << 0) | (1 << 6);

        _ = this.Database.Seed(BuildSchedule(150, this.Ours, new TimeSpan(6, 0, 0), daysOfWeek: _SaturdayAndSunday));

        await this.HandleAsync();

        this.m_SceneLogic.Verify(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SkipsAScheduleThatAlreadyRanToday()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, new TimeSpan(6, 0, 0),
            lastRunUTC: TestServiceFactory.DefaultNow.UtcDateTime.AddHours(-2)));

        await this.HandleAsync();

        this.m_SceneLogic.Verify(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_FiresAgainOnANewDay()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, new TimeSpan(6, 0, 0),
            lastRunUTC: TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-1)));

        await this.HandleAsync();

        this.m_SceneLogic.Verify(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTheLightsCannotBeReached_LeavesTheMarkerSoTheNextTickRetries()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, new TimeSpan(6, 0, 0)));

        await this.HandleAsync(LightCommandResult.Unavailable);

        this.m_OutputPort.Verify(o => o.PresentSchedulesRunAsync(0, 1, It.IsAny<CancellationToken>()), Times.Once);
        _ = this.Stored<LightSchedule>().Single().LastRunUTC.Should().BeNull(
            "leaving the marker alone is what makes the next tick retry rather than skip the day");
    }

    [Fact]
    public async Task HandleAsync_RunsForEveryHouseholdBecauseThereIsNobodySignedInBehindATimer()
    {
        _ = this.Database.Seed(
            BuildSchedule(150, this.Ours, new TimeSpan(6, 0, 0)),
            BuildSchedule(950, this.Theirs, new TimeSpan(6, 0, 0)));

        await this.HandleAsync();

        this.m_OutputPort.Verify(o => o.PresentSchedulesRunAsync(2, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsDue_SaysSoWithoutTouchingTheLights()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, new TimeSpan(21, 0, 0)));

        await this.HandleAsync();

        this.m_OutputPort.Verify(o => o.PresentSchedulesRunAsync(0, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion Methods

}
