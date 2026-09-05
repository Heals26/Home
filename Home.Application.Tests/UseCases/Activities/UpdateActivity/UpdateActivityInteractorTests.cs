using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.UpdateActivity;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.Activities.UpdateActivity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.Activities.UpdateActivity;

/// <summary>
/// Editing a card. Seven properties travel through change trackers, and the two that carry IDs are
/// looked up inside the household so a guessed one misses.
/// </summary>
public class UpdateActivityInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<Activity>> m_AuditLogic = new();
    private readonly UpdateActivityPresenter m_Presenter = new(Mapper);

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
            Sequence = 1
        };

    private Activity BuildCard(long activityID, Household household, ActivityState? state = null, User? user = null)
        => new()
        {
            ActivityID = activityID,
            DueDateUTC = new DateTime(2026, 8, 10),
            DueTime = new TimeSpan(8, 0, 0),
            Household = household,
            Sequence = 5,
            State = state,
            Title = $"Card {activityID}",
            User = user
        };

    private Task HandleAsync(
        long activityID,
        PropertyChangeTracker<string> title = default,
        PropertyChangeTracker<DateTime?> dueDateUTC = default,
        PropertyChangeTracker<TimeSpan?> dueTime = default,
        PropertyChangeTracker<DateTime?> completedDateUTC = default,
        PropertyChangeTracker<int> sequence = default,
        PropertyChangeTracker<long?> stateID = default,
        PropertyChangeTracker<long?> userID = default)
    {
        var _Services = this.Services(out var _Context);

        return new UpdateActivityInteractor().HandleAsync(
            new UpdateActivityInputPort(activityID, title, dueDateUTC, dueTime, completedDateUTC, sequence, stateID, userID),
            this.m_Presenter,
            _Services
                .With(this.m_AuditLogic.Object)
                .With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_RenamesTheCardAndSavesIt()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, title: new("Clean the balcony"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Activity>().Single().Title.Should().Be("Clean the balcony");
    }

    [Fact]
    public async Task HandleAsync_LeavesEveryPropertyNobodySent()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, title: new("Renamed"));

        var _Stored = this.Stored<Activity>().Single();

        _ = _Stored.DueDateUTC.Should().Be(new DateTime(2026, 8, 10));
        _ = _Stored.DueTime.Should().Be(new TimeSpan(8, 0, 0));
        _ = _Stored.Sequence.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_CanClearADueDateRatherThanOnlySetOne()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, dueDateUTC: new(null), dueTime: new(null));

        var _Stored = this.Stored<Activity>().Single();

        _ = _Stored.DueDateUTC.Should().BeNull();
        _ = _Stored.DueTime.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_MovingToAFinishedColumnStampsTheCard()
    {
        _ = this.Database.Seed(
            BuildColumn(121, this.Ours, "Done", isComplete: true),
            this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, stateID: new(121));

        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_TakingACardOutOfEveryColumnClearsItsCompletion()
    {
        var _Done = BuildColumn(121, this.Ours, "Done", isComplete: true);
        var _Card = this.BuildCard(100, this.Ours, _Done);
        _Card.CompletedDateUTC = TestServiceFactory.DefaultNow.UtcDateTime;

        _ = this.Database.Seed(_Card);

        await this.HandleAsync(100, stateID: new(null));

        _ = this.Stored<Activity>().Count(a => a.State == null).Should().Be(1);
        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheColumnBelongsToAnotherHousehold_TakesTheCardOutOfEveryColumnInstead()
    {
        var _ToDo = BuildColumn(120, this.Ours, "To do");

        _ = this.Database.Seed(
            BuildColumn(920, this.Theirs, "Theirs"),
            this.BuildCard(100, this.Ours, _ToDo));

        await this.HandleAsync(100, stateID: new(920));

        _ = this.Stored<Activity>().Count(a => a.State == null).Should().Be(
            1,
            "a guessed column ID misses, and a miss is the same as sending none");
    }

    [Fact]
    public async Task HandleAsync_WillNotAssignTheCardToSomebodyInAnotherHousehold()
    {
        _ = this.Database.Seed(this.Member, this.Neighbour, this.BuildCard(100, this.Ours, user: this.Member));

        await this.HandleAsync(100, userID: new(this.Neighbour.UserID));

        _ = this.Stored<Activity>().Count(a => a.User == null).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheCardBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(this.BuildCard(900, this.Theirs));

        await this.HandleAsync(900, title: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Activity>().Single().Title.Should().Be("Card 900");
    }

    [Fact]
    public async Task HandleAsync_RecordsThatTheCardChanged()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, title: new("Renamed"));

        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<Activity>()), Times.Once);
    }

    #endregion Methods

}
