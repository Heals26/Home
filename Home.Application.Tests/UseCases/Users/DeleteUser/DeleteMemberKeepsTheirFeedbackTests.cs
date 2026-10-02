using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Users.DeleteUser;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Users.DeleteUser;

namespace Home.Application.Tests.UseCases.Users.DeleteUser;

/// <summary>
/// Feedback outlives the member who sent it. Its link to them is NoAction, so removing a member
/// unhooks it: what was said stays, only who said it goes.
/// </summary>
public class DeleteMemberKeepsTheirFeedbackTests : InteractorTest
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
    public async Task HandleAsync_KeepsTheirFeedbackWithoutNamingThem()
    {
        var _Bo = new User() { UserID = 102, FirstName = "Bo", Household = this.Ours, LastName = "Member" };

        _ = this.Database.Seed(
            new Feedback() { FeedbackID = 150, Content = "The timer is too quiet", Household = this.Ours, Kind = FeedbackKind.Problem, User = _Bo },
            new Feedback() { FeedbackID = 151, Content = "A pantry list", Household = this.Ours, Kind = FeedbackKind.Idea, User = this.Member });

        await this.HandleAsync(_Bo.UserID);

        _ = this.Stored<User>().Any(u => u.UserID == _Bo.UserID).Should().BeFalse();
        _ = this.Stored<Feedback>().Count().Should().Be(2, "feedback outlives whoever sent it");
        _ = this.Stored<Feedback>().Count(f => f.User != null && f.User.UserID == _Bo.UserID).Should().Be(0);
        _ = this.Stored<Feedback>().Count(f => f.User != null && f.User.UserID == this.Member.UserID).Should().Be(1, "feedback from a member who stays is untouched");
    }

    #endregion Methods

}
