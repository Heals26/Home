using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityRegions.GetActivityRegions;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityRegions.GetActivityRegions;
using Home.WebApi.UseCases.ActivityRegions.GetActivityRegions;

namespace Home.Application.Tests.UseCases.ActivityRegions.GetActivityRegions;

/// <summary>
/// The sections on one card. Carried the same unprojected-section fault as
/// <c>GetActivityInteractor</c>, because the presenter here reads the heading too.
/// </summary>
public class GetActivityRegionsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetActivityRegionsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard()
    {
        var _Activity = new Activity() { ActivityID = 100, Household = this.Ours, Title = "Clean the balcony" };

        // Seeded in the wrong order, so the presenter has to be the one sorting them.
        _Activity.Regions =
        [
            new ActivityRegion()
            {
                ActivityRegionID = 131,
                Activity = _Activity,
                CardSection = new CardSection() { CardSectionID = 111, Household = this.Ours, Name = "Steps", Sequence = 2 },
                Sequence = 2
            },
            new ActivityRegion()
            {
                ActivityRegionID = 130,
                Activity = _Activity,
                CardSection = new CardSection() { CardSectionID = 110, Household = this.Ours, Name = "Details", Sequence = 1 },
                Sequence = 1
            }
        ];

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

    private Task HandleAsync(long activityID)
        => new GetActivityRegionsInteractor().HandleAsync(
            new GetActivityRegionsInputPort(activityID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_NamesTheSectionBehindEveryRegionInSequence()
    {
        _ = this.Database.Seed(this.BuildCard());

        await this.HandleAsync(100);

        var _Regions = Ok<GetActivityRegionsApiResponse>(this.m_Presenter).Regions;

        _ = _Regions.Select(r => r.ActivityRegionID).Should().Equal(130, 131);
        _ = _Regions.Select(r => r.CardSectionName).Should().Equal(
            ["Details", "Steps"],
            "the presenter reads the section off every region, so the query has to load it");
        _ = _Regions.Select(r => r.CardSectionID).Should().Equal(110, 111);
    }

    [Fact]
    public async Task HandleAsync_WhenTheCardBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard(), this.BuildTheirCard());

        await this.HandleAsync(900);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchCardExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard());

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
