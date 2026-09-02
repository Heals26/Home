using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityContents.GetActivityContent;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityContents.GetActivityContent;
using Home.WebApi.UseCases.ActivityContents.GetActivityContent;

namespace Home.Application.Tests.UseCases.ActivityContents.GetActivityContent;

/// <summary>
/// A single line on a card. Its household is two navigations away — through the region to the
/// activity — which is the longest ownership path any read in the application walks.
/// </summary>
public class GetActivityContentInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetActivityContentPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, long regionID, long contentID, Household household, string content)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = household, Title = $"Card {activityID}" };

        var _Region = new ActivityRegion()
        {
            ActivityRegionID = regionID,
            Activity = _Activity,
            CardSection = new CardSection() { CardSectionID = regionID + 1000, Household = household, Name = "Details", Sequence = 1 },
            Sequence = 1
        };

        _Region.Fields = [new ActivityContent() { ActivityContentID = contentID, Content = content, Region = _Region, Sequence = 3 }];
        _Activity.Regions = [_Region];

        return _Activity;
    }

    private Task HandleAsync(long activityContentID)
        => new GetActivityContentInteractor().HandleAsync(
            new GetActivityContentInputPort(activityContentID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WhenTheLineIsOurs_PresentsIt()
    {
        _ = this.Database.Seed(this.BuildCard(100, 130, 140, this.Ours, "Get cleaning broom"));

        await this.HandleAsync(140);

        var _Response = Ok<GetActivityContentApiResponse>(this.m_Presenter);

        _ = _Response.ActivityContentID.Should().Be(140);
        _ = _Response.Content.Should().Be("Get cleaning broom");
        _ = _Response.Sequence.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_WhenTheLineBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, 130, 140, this.Ours, "Get cleaning broom"),
            this.BuildCard(900, 930, 940, this.Theirs, "Private"));

        await this.HandleAsync(940);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchLineExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard(100, 130, 140, this.Ours, "Get cleaning broom"));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
