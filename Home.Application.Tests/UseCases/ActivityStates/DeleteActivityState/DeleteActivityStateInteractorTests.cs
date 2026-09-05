using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityStates.DeleteActivityState;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityStates.DeleteActivityState;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ActivityStates.DeleteActivityState;

/// <summary>
/// Removing a column, which is the most guarded write on the board: the cards in it have to go
/// somewhere, the last column may not be removed at all, and another device could drop a card in
/// while the move is running.
/// </summary>
public class DeleteActivityStateInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteActivityStatePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ActivityState BuildColumn(long activityStateID, Household household, string name, bool isComplete = false)
        => new()
        {
            Activities = [],
            ActivityStateID = activityStateID,
            Household = household,
            IsComplete = isComplete,
            Name = name,
            Sequence = (int)activityStateID
        };

    private Activity BuildCard(long activityID, ActivityState state)
        => new()
        {
            ActivityID = activityID,
            Household = this.Ours,
            State = state,
            Title = $"Card {activityID}"
        };

    private Task HandleAsync(long activityStateID, long moveCardsToStateID)
    {
        var _Services = this.Services(out var _Context);

        return new DeleteActivityStateInteractor().HandleAsync(
            new DeleteActivityStateInputPort(activityStateID, moveCardsToStateID),
            this.m_Presenter,
            _Services.With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_MovesTheCardsToTheTargetColumnAndThenDeletes()
    {
        var _Doing = BuildColumn(120, this.Ours, "Doing");
        var _ToDo = BuildColumn(121, this.Ours, "To do");

        _ = this.Database.Seed(_ToDo, this.BuildCard(100, _Doing), this.BuildCard(101, _Doing));

        await this.HandleAsync(120, 121);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ActivityState>().Select(s => s.ActivityStateID).Should().Equal([121]);
        _ = this.Stored<Activity>().Count(a => a.State!.ActivityStateID == 121).Should().Be(
            2,
            "the cards go somewhere rather than down with the column");
    }

    [Fact]
    public async Task HandleAsync_WhenTheTargetColumnIsComplete_StampsTheCardsItInherits()
    {
        var _Doing = BuildColumn(120, this.Ours, "Doing");
        var _Done = BuildColumn(121, this.Ours, "Done", isComplete: true);

        _ = this.Database.Seed(_Done, this.BuildCard(100, _Doing));

        await this.HandleAsync(120, 121);

        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().Be(
            TestServiceFactory.DefaultNow.UtcDateTime,
            "landing in a finished column is what stamps a card, however it got there");
    }

    [Fact]
    public async Task HandleAsync_WhenItIsTheOnlyColumn_RefusesRatherThanLeavingTheBoardUnusable()
    {
        _ = this.Database.Seed(BuildColumn(120, this.Ours, "Doing"));

        await this.HandleAsync(120, 121);

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<ActivityState>().Should().ContainSingle(
            "a board with no columns has nowhere to put a card and no way back through the UI");
    }

    [Fact]
    public async Task HandleAsync_WhenTheTargetColumnDoesNotExist_PresentsNotFoundAndKeepsEverything()
    {
        _ = this.Database.Seed(
            BuildColumn(120, this.Ours, "Doing"),
            BuildColumn(121, this.Ours, "To do"));

        await this.HandleAsync(120, 404);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityState>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WillNotMoveTheCardsIntoTheColumnBeingDeleted()
    {
        _ = this.Database.Seed(
            BuildColumn(120, this.Ours, "Doing"),
            BuildColumn(121, this.Ours, "To do"));

        await this.HandleAsync(120, 120);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityState>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WillNotMoveTheCardsIntoAnotherHouseholdsColumn()
    {
        _ = this.Database.Seed(
            BuildColumn(120, this.Ours, "Doing"),
            BuildColumn(121, this.Ours, "To do"),
            BuildColumn(920, this.Theirs, "Theirs"));

        await this.HandleAsync(120, 920);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityState>().Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_WhenTheColumnBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildColumn(120, this.Ours, "Doing"),
            BuildColumn(920, this.Theirs, "Theirs"),
            BuildColumn(921, this.Theirs, "Also theirs"));

        await this.HandleAsync(920, 921);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityState>().Should().HaveCount(3);
    }

    #endregion Methods

}
