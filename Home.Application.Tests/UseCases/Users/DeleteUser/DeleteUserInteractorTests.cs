using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Users.DeleteUser;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Users.DeleteUser;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Users.DeleteUser;

/// <summary>
/// Removing a member who has left. The 15 Aug decision set the audit relationship to SetNull
/// specifically so this could happen without taking the household's history with it.
/// </summary>
public class DeleteUserInteractorTests : InteractorTest
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
    public async Task HandleAsync_RemovesOurMember()
    {
        _ = this.Database.Seed(this.Member, this.Neighbour);

        await this.HandleAsync(this.Member.UserID);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<User>().Select(u => u.UserID).Should().Equal([this.Neighbour.UserID]);
    }

    [Fact]
    public async Task HandleAsync_WhenTheMemberIsInAnotherHousehold_KeepsThemAndStillAnswersNoContent()
    {
        _ = this.Database.Seed(this.Member, this.Neighbour);

        await this.HandleAsync(this.Neighbour.UserID);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<User>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchMemberExists_AnswersNoContentSoDeletingTwiceIsHarmless()
    {
        _ = this.Database.Seed(this.Member);

        await this.HandleAsync(404);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<User>().Should().ContainSingle();
    }

    #endregion Methods

}
