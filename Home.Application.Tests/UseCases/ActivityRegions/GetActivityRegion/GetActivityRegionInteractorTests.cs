using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityRegions.GetActivityRegion;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityRegions.GetActivityRegion;
using Home.WebApi.UseCases.ActivityRegions.GetActivityRegion;

namespace Home.Application.Tests.UseCases.ActivityRegions.GetActivityRegion;

/// <summary>
/// One section of one card, with its lines. The presenter reads both the heading and the lines,
/// so the query has to load both.
/// </summary>
public class GetActivityRegionInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetActivityRegionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard()
    {
        var _Activity = new Activity() { ActivityID = 100, Household = this.Ours, Title = "Clean the balcony" };

        var _Region = new ActivityRegion()
        {
            ActivityRegionID = 130,
            Activity = _Activity,
            CardSection = new CardSection() { CardSectionID = 110, Household = this.Ours, Name = "Details", Sequence = 1 },
            Sequence = 1
        };

        _Region.Fields =
        [
            new ActivityContent() { ActivityContentID = 141, Content = "Get cleaning material", Region = _Region, Sequence = 2 },
            new ActivityContent() { ActivityContentID = 140, Content = "Get cleaning broom", Region = _Region, Sequence = 1 }
        ];

        _Activity.Regions = [_Region];

        return _Activity;
    }

    private Activity BuildTheirCard()
    {
        var _Activity = new Activity() { ActivityID = 900, Household = this.Theirs, Title = "Not ours" };

        _Activity.Regions =
        [
            new ActivityRegion()
            {
                ActivityRegionID = 930,
                Activity = _Activity,
                CardSection = new CardSection() { CardSectionID = 910, Household = this.Theirs, Name = "Theirs", Sequence = 1 },
                Sequence = 1
            }
        ];

        return _Activity;
    }

    private Task HandleAsync(long activityRegionID)
        => new GetActivityRegionInteractor().HandleAsync(
            new GetActivityRegionInputPort(activityRegionID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_NamesTheSectionAndReturnsItsLinesInSequence()
    {
        _ = this.Database.Seed(this.BuildCard());

        await this.HandleAsync(130);

        var _Response = Ok<GetActivityRegionApiResponse>(this.m_Presenter);

        _ = _Response.CardSectionID.Should().Be(110);
        _ = _Response.CardSectionName.Should().Be(
            "Details",
            "the presenter reads the section name, so the query has to load it");
        _ = _Response.Fields.Select(f => f.Content).Should().Equal("Get cleaning broom", "Get cleaning material");
    }

    [Fact]
    public async Task HandleAsync_WhenTheRegionBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard(), this.BuildTheirCard());

        await this.HandleAsync(930);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchRegionExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard());

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
