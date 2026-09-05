using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.SetActivityCompletion;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.Activities.SetActivityCompletion;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.Activities.SetActivityCompletion;

/// <summary>
/// Ticking a chore off from anywhere. The caller says only whether it is finished; which column
/// that means is the household's own decision, so the board, the week, the day and the dashboard
/// all behave the same without any of them knowing the board's shape.
/// </summary>
public class SetActivityCompletionInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<Activity>> m_AuditLogic = new();
    private readonly SetActivityCompletionPresenter m_Presenter = new(Mapper);

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

    private Activity BuildCard(long activityID, Household household, ActivityState? state, DateTime? completedDateUTC = null)
        => new()
        {
            ActivityID = activityID,
            CompletedDateUTC = completedDateUTC,
            Household = household,
            State = state,
            Title = $"Card {activityID}"
        };

    private Task HandleAsync(long activityID, bool isComplete)
    {
        var _Services = this.Services(out var _Context);

        return new SetActivityCompletionInteractor().HandleAsync(
            new SetActivityCompletionInputPort(activityID, isComplete),
            this.m_Presenter,
            _Services
                .With(this.m_AuditLogic.Object)
                .With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_TickingOffMovesTheCardToTheFirstFinishedColumn()
    {
        var _ToDo = BuildColumn(120, this.Ours, "To do", 0);

        _ = this.Database.Seed(
            BuildColumn(122, this.Ours, "Also done", 3, isComplete: true),
            BuildColumn(121, this.Ours, "Done", 2, isComplete: true),
            this.BuildCard(100, this.Ours, _ToDo));

        await this.HandleAsync(100, isComplete: true);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();

        _ = this.Stored<Activity>().Count(a => a.State != null && a.State.ActivityStateID == 121).Should().Be(
            1,
            "the leftmost finished column is the one it lands in");
        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_UntickingMovesTheCardBackToTheFirstUnfinishedColumn()
    {
        var _Done = BuildColumn(121, this.Ours, "Done", 2, isComplete: true);

        _ = this.Database.Seed(
            BuildColumn(120, this.Ours, "To do", 0),
            this.BuildCard(100, this.Ours, _Done, TestServiceFactory.DefaultNow.UtcDateTime));

        await this.HandleAsync(100, isComplete: false);

        _ = this.Stored<Activity>().Count(a => a.State != null && a.State.ActivityStateID == 120).Should().Be(1);
        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheBoardHasNoFinishedColumn_StillTicksTheCardOff()
    {
        var _ToDo = BuildColumn(120, this.Ours, "To do", 0);

        _ = this.Database.Seed(this.BuildCard(100, this.Ours, _ToDo));

        await this.HandleAsync(100, isComplete: true);

        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().Be(
            TestServiceFactory.DefaultNow.UtcDateTime,
            "a board with no column of that kind still has to be able to tick something off");
        _ = this.Stored<Activity>().Count(a => a.State != null && a.State.ActivityStateID == 120).Should().Be(
            1,
            "and the card stays where it was");
    }

    [Fact]
    public async Task HandleAsync_NeverMovesACardIntoAnotherHouseholdsColumn()
    {
        var _ToDo = BuildColumn(120, this.Ours, "To do", 0);

        _ = this.Database.Seed(
            BuildColumn(920, this.Theirs, "Their done", 0, isComplete: true),
            this.BuildCard(100, this.Ours, _ToDo));

        await this.HandleAsync(100, isComplete: true);

        _ = this.Stored<Activity>().Count(a => a.State != null && a.State.ActivityStateID == 120).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheCardBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(
            BuildColumn(121, this.Ours, "Done", 2, isComplete: true),
            this.BuildCard(900, this.Theirs, null));

        await this.HandleAsync(900, isComplete: true);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_RecordsThatTheCardChanged()
    {
        var _ToDo = BuildColumn(120, this.Ours, "To do", 0);

        _ = this.Database.Seed(this.BuildCard(100, this.Ours, _ToDo));

        await this.HandleAsync(100, isComplete: true);

        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<Activity>()), Times.Once);
    }

    #endregion Methods

}
