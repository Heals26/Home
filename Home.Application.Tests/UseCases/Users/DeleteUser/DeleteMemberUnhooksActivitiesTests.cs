using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Users.DeleteUser;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Users.DeleteUser;

namespace Home.Application.Tests.UseCases.Users.DeleteUser;

/// <summary>
/// Both links from a chore to a member are NoAction in the database, so removing a member has to
/// unhook the chores itself: the work survives, it just stops naming someone who is gone.
/// </summary>
public class DeleteMemberUnhooksActivitiesTests : InteractorTest
{

    #region Fields

    private readonly DeleteUserPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(long userID)
        => new DeleteUserInteractor().HandleAsync(
            new DeleteUserInputPort(userID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ClearsTheAssigneeAndWhoCompletedFromTheirChores()
    {
        var _Bo = new User() { UserID = 102, FirstName = "Bo", Household = this.Ours, LastName = "Member" };

        _ = this.Database.Seed(
            new Activity() { ActivityID = 150, Household = this.Ours, Title = "Bins", User = _Bo },
            new Activity() { ActivityID = 151, Household = this.Ours, Title = "Dishes", CompletedByUser = _Bo, CompletedDateUTC = new DateTime(2026, 9, 1), User = this.Member },
            new CalendarEvent() { CalendarEventID = 160, Household = this.Ours, Title = "Swimming", StartDate = new DateOnly(2026, 9, 10), EndDate = new DateOnly(2026, 9, 10), IsAllDay = true, Members = [new CalendarEventMember() { User = _Bo }] });

        await this.HandleAsync(_Bo.UserID);

        _ = this.Stored<User>().Any(u => u.UserID == _Bo.UserID).Should().BeFalse();
        _ = this.Stored<Activity>().Count(a => a.User != null && a.User.UserID == _Bo.UserID).Should().Be(0);
        _ = this.Stored<Activity>().Count(a => a.CompletedByUser != null && a.CompletedByUser.UserID == _Bo.UserID).Should().Be(0);
        _ = this.Stored<Activity>().Count(a => a.User != null && a.User.UserID == this.Member.UserID).Should().Be(1, "Ada still has her chore");
        _ = this.Stored<Activity>().Count(a => a.CompletedDateUTC != null).Should().Be(1, "the chore stays done, it just stops naming who did it");
        _ = this.Stored<CalendarEventMember>().Should().BeEmpty();
    }

    #endregion Methods

}
