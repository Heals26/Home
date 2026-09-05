using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityContents.DeleteActivityContent;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityContents.DeleteActivityContent;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ActivityContents.DeleteActivityContent;

/// <summary>
/// Rubbing out one line on a card. Reached through the region to the activity to the household,
/// the longest ownership path any write in the application walks.
/// </summary>
public class DeleteActivityContentInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteActivityContentPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, Household household, params long[] contentIDs)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = household, Title = $"Card {activityID}" };

        var _Region = new ActivityRegion()
        {
            ActivityRegionID = activityID + 30,
            Activity = _Activity,
            CardSection = new CardSection() { CardSectionID = activityID + 1000, Household = household, Name = "Details", Sequence = 1 },
            Sequence = 1
        };

        _Region.Fields =
        [
            .. contentIDs.Select((id, index) => new ActivityContent()
            {
                ActivityContentID = id,
                Content = $"Line {id}",
                Region = _Region,
                Sequence = index + 1
            })
        ];

        _Activity.Regions = [_Region];

        return _Activity;
    }

    private Task HandleAsync(long activityContentID)
        => new DeleteActivityContentInteractor().HandleAsync(
            new DeleteActivityContentInputPort(activityContentID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesOnlyThatLine()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 140, 141));

        await this.HandleAsync(140);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ActivityContent>().Select(c => c.ActivityContentID).Should().Equal([141]);
    }

    [Fact]
    public async Task HandleAsync_LeavesTheSectionAndTheCardStanding()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 140));

        await this.HandleAsync(140);

        _ = this.Stored<ActivityRegion>().Should().ContainSingle();
        _ = this.Stored<Activity>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheLineBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, this.Ours, 140),
            this.BuildCard(900, this.Theirs, 940));

        await this.HandleAsync(940);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityContent>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchLineExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 140));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
