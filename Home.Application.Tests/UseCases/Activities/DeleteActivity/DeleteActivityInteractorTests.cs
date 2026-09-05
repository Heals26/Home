using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.DeleteActivity;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.Activities.DeleteActivity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.Activities.DeleteActivity;

/// <summary>
/// Throwing a card away. This one answers no content whatever happens, including for a card that
/// does not exist and one belonging to another household, so deleting twice is harmless and a
/// caller cannot use the response to find out whose cards exist.
/// </summary>
public class DeleteActivityInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<Activity>> m_AuditLogic = new();
    private readonly DeleteActivityPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Activity BuildCard(long activityID, Household household)
        => new()
        {
            ActivityID = activityID,
            Household = household,
            Title = $"Card {activityID}"
        };

    private Task HandleAsync(long activityID)
        => new DeleteActivityInteractor().HandleAsync(
            new DeleteActivityInputPort(activityID),
            this.m_Presenter,
            this.Services().With(this.m_AuditLogic.Object).Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesOurCard()
    {
        _ = this.Database.Seed(BuildCard(100, this.Ours), BuildCard(101, this.Ours));

        await this.HandleAsync(100);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Activity>().Select(a => a.ActivityID).Should().Equal([101]);
    }

    [Fact]
    public async Task HandleAsync_WhenTheCardBelongsToAnotherHousehold_KeepsItAndStillAnswersNoContent()
    {
        _ = this.Database.Seed(BuildCard(100, this.Ours), BuildCard(900, this.Theirs));

        await this.HandleAsync(900);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>(
            "answering the same either way is what stops the response revealing whose cards exist");
        _ = this.Stored<Activity>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchCardExists_AnswersNoContentSoDeletingTwiceIsHarmless()
    {
        _ = this.Database.Seed(BuildCard(100, this.Ours));

        await this.HandleAsync(404);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Activity>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_RecordsThatOurCardWasDeleted()
    {
        _ = this.Database.Seed(BuildCard(100, this.Ours));

        await this.HandleAsync(100);

        this.m_AuditLogic.Verify(a => a.DeleteAudit(It.Is<Activity>(x => x.ActivityID == 100)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RecordsNothingForACardItDidNotDelete()
    {
        _ = this.Database.Seed(BuildCard(900, this.Theirs));

        await this.HandleAsync(900);

        this.m_AuditLogic.Verify(a => a.DeleteAudit(It.IsAny<Activity>()), Times.Never);
    }

    #endregion Methods

}
