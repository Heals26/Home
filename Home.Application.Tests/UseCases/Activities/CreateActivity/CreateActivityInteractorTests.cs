using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.CreateActivity;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.Activities.CreateActivity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.Activities.CreateActivity;

/// <summary>
/// Putting a card on the board. Both IDs the caller may send, the column and the assignee, are
/// looked up inside the household, so a guessed one has to miss rather than land on another
/// family's board.
/// </summary>
public class CreateActivityInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<Activity>> m_AuditLogic = new();
    private readonly CreateActivityPresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(string title, long? stateID = null, long? userID = null, DateTime? dueDateUTC = null, TimeSpan? dueTime = null)
    {
        var _Services = this.Services(out var _Context);

        return new CreateActivityInteractor().HandleAsync(
            new CreateActivityInputPort(title, dueDateUTC, dueTime, stateID, userID),
            this.m_Presenter,
            _Services
                .With(this.m_AuditLogic.Object)
                .With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_WritesTheCardToTheSignedInHousehold()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Clean the balcony", dueDateUTC: new DateTime(2026, 8, 12), dueTime: new TimeSpan(9, 0, 0));

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<Activity>().Single();

        _ = _Stored.Title.Should().Be("Clean the balcony");
        _ = _Stored.DueDateUTC.Should().Be(new DateTime(2026, 8, 12));
        _ = _Stored.DueTime.Should().Be(new TimeSpan(9, 0, 0));
        _ = this.Stored<Activity>().Count(a => a.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_PutsTheCardInTheColumnAskedFor()
    {
        _ = this.Database.Seed(BuildColumn(120, this.Ours, "Doing"));

        await this.HandleAsync("Clean the balcony", stateID: 120);

        _ = this.Stored<Activity>().Count(a => a.State != null && a.State.ActivityStateID == 120).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheColumnIsFinished_StampsTheNewCard()
    {
        _ = this.Database.Seed(BuildColumn(120, this.Ours, "Done", isComplete: true));

        await this.HandleAsync("Already done", stateID: 120);

        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_WhenTheColumnBelongsToAnotherHousehold_LeavesTheCardWithoutOne()
    {
        _ = this.Database.Seed(BuildColumn(920, this.Theirs, "Theirs"));

        await this.HandleAsync("Clean the balcony", stateID: 920);

        _ = this.Stored<Activity>().Count(a => a.State == null).Should().Be(
            1,
            "a guessed column ID has to miss rather than land on another family's board");
    }

    [Fact]
    public async Task HandleAsync_AssignsTheCardToAMemberOfOurHousehold()
    {
        _ = this.Database.Seed(this.Member);

        await this.HandleAsync("Clean the balcony", userID: this.Member.UserID);

        _ = this.Stored<Activity>().Count(a => a.User != null && a.User.UserID == this.Member.UserID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WillNotAssignTheCardToSomebodyInAnotherHousehold()
    {
        _ = this.Database.Seed(this.Member, this.Neighbour);

        await this.HandleAsync("Clean the balcony", userID: this.Neighbour.UserID);

        _ = this.Stored<Activity>().Count(a => a.User == null).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_RecordsThatTheCardWasCreated()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Clean the balcony");

        this.m_AuditLogic.Verify(a => a.AddAudit(It.IsAny<Activity>()), Times.Once);
    }

    #endregion Methods

}
