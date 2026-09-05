using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityStates.UpdateActivityState;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityStates.UpdateActivityState;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ActivityStates.UpdateActivityState;

/// <summary>
/// Renaming or moving a column, and the interesting one: turning the finished flag on or off has
/// to catch up the cards already sitting in it, or the dashboard keeps listing chores the family
/// has stopped thinking about.
/// </summary>
public class UpdateActivityStateInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateActivityStatePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ActivityState BuildColumn(long activityStateID, Household household, string name, int sequence, bool isComplete = false)
        => new()
        {
            Activities = [],
            ActivityStateID = activityStateID,
            Household = household,
            IsComplete = isComplete,
            Name = name,
            Sequence = sequence
        };

    private Activity BuildCard(long activityID, ActivityState state, DateTime? completedDateUTC = null)
        => new()
        {
            ActivityID = activityID,
            CompletedDateUTC = completedDateUTC,
            Household = this.Ours,
            State = state,
            Title = $"Card {activityID}"
        };

    private Task HandleAsync(
        long activityStateID,
        PropertyChangeTracker<bool> isComplete = default,
        PropertyChangeTracker<string> name = default,
        PropertyChangeTracker<int> sequence = default)
    {
        var _Services = this.Services(out var _Context);

        return new UpdateActivityStateInteractor().HandleAsync(
            new UpdateActivityStateInputPort(activityStateID, isComplete, name, sequence),
            this.m_Presenter,
            _Services.With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_RenamesTheColumnAndTrimsIt()
    {
        _ = this.Database.Seed(BuildColumn(120, this.Ours, "Doing", 1));

        await this.HandleAsync(120, name: new("  Progressing  "));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ActivityState>().Single().Name.Should().Be("Progressing");
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheNameIsSent_LeavesTheSequenceAndTheFinishedFlagAlone()
    {
        _ = this.Database.Seed(BuildColumn(120, this.Ours, "Done", 3, isComplete: true));

        await this.HandleAsync(120, name: new("Finished"));

        var _Stored = this.Stored<ActivityState>().Single();

        _ = _Stored.Sequence.Should().Be(3);
        _ = _Stored.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenAColumnBecomesFinished_StampsTheCardsAlreadyInIt()
    {
        var _Column = BuildColumn(120, this.Ours, "Doing", 1);

        _ = this.Database.Seed(this.BuildCard(100, _Column), this.BuildCard(101, _Column));

        await this.HandleAsync(120, isComplete: new(true));

        _ = this.Stored<Activity>().Select(a => a.CompletedDateUTC).Should().AllBeEquivalentTo(
            TestServiceFactory.DefaultNow.UtcDateTime,
            "the dashboard stops listing a chore because its card is stamped, not because its column is");
    }

    [Fact]
    public async Task HandleAsync_WhenAColumnStopsBeingFinished_ClearsTheCardsAlreadyInIt()
    {
        var _Column = BuildColumn(120, this.Ours, "Done", 1, isComplete: true);

        _ = this.Database.Seed(this.BuildCard(100, _Column, TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-2)));

        await this.HandleAsync(120, isComplete: new(false));

        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheFinishedFlagIsSentUnchanged_LeavesTheCardsAlone()
    {
        var _Column = BuildColumn(120, this.Ours, "Done", 1, isComplete: true);
        var _Stamped = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-2);

        _ = this.Database.Seed(this.BuildCard(100, _Column, _Stamped));

        await this.HandleAsync(120, isComplete: new(true));

        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().Be(
            _Stamped,
            "a card already carrying a date keeps it, rather than being restamped with now");
    }

    [Fact]
    public async Task HandleAsync_DoesNotTouchCardsInOtherColumns()
    {
        var _Doing = BuildColumn(120, this.Ours, "Doing", 1);
        var _ToDo = BuildColumn(121, this.Ours, "To do", 0);

        _ = this.Database.Seed(this.BuildCard(100, _Doing), this.BuildCard(101, _ToDo));

        await this.HandleAsync(120, isComplete: new(true));

        _ = this.Stored<Activity>().Single(a => a.ActivityID == 101).CompletedDateUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheColumnBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(
            BuildColumn(120, this.Ours, "Doing", 1),
            BuildColumn(920, this.Theirs, "Theirs", 1));

        await this.HandleAsync(920, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityState>().Single(s => s.ActivityStateID == 920).Name.Should().Be("Theirs");
    }

    #endregion Methods

}
