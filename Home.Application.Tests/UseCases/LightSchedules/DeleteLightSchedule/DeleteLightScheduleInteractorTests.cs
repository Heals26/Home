using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightSchedules.DeleteLightSchedule;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.LightSchedules.DeleteLightSchedule;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.LightSchedules.DeleteLightSchedule;

/// <summary>
/// Removing a schedule. The scene it applied stays: a schedule is a timer, not the look itself.
/// </summary>
public class DeleteLightScheduleInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteLightSchedulePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static LightSchedule BuildSchedule(long lightScheduleID, Household household, string name)
        => new()
        {
            DaysOfWeek = 127,
            IsEnabled = true,
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
            TimeOfDay = new TimeSpan(21, 0, 0),
            Trigger = LightScheduleTrigger.Time
        };

    private Task HandleAsync(long lightScheduleID)
        => new DeleteLightScheduleInteractor().HandleAsync(
            new DeleteLightScheduleInputPort(lightScheduleID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheScheduleAndLeavesTheSceneStanding()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, "Lights down"));

        await this.HandleAsync(150);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<LightSchedule>().Should().BeEmpty();
        _ = this.Stored<LightScene>().Should().ContainSingle("a schedule is a timer, not the look itself");
    }

    [Fact]
    public async Task HandleAsync_WhenTheScheduleBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, "Lights down"), BuildSchedule(950, this.Theirs, "Theirs"));

        await this.HandleAsync(950);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<LightSchedule>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchScheduleExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildSchedule(150, this.Ours, "Lights down"));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
