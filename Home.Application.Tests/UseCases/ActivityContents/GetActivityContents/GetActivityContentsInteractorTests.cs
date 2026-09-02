using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityContents.GetActivityContents;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityContents.GetActivityContents;
using Home.WebApi.UseCases.ActivityContents.GetActivityContents;

namespace Home.Application.Tests.UseCases.ActivityContents.GetActivityContents;

/// <summary>
/// The lines written under one section. The projection is the only thing loading them, and an
/// unloaded collection is worse than a crash here: the section simply reads as empty.
/// </summary>
public class GetActivityContentsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetActivityContentsPresenter m_Presenter = new(Mapper);

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

        var _Region = new ActivityRegion()
        {
            ActivityRegionID = 930,
            Activity = _Activity,
            CardSection = new CardSection() { CardSectionID = 910, Household = this.Theirs, Name = "Theirs", Sequence = 1 },
            Sequence = 1
        };

        _Region.Fields = [new ActivityContent() { ActivityContentID = 940, Content = "Private", Region = _Region, Sequence = 1 }];
        _Activity.Regions = [_Region];

        return _Activity;
    }

    private Task HandleAsync(long activityRegionID)
        => new GetActivityContentsInteractor().HandleAsync(
            new GetActivityContentsInputPort(activityRegionID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsEveryLineUnderTheSectionInSequence()
    {
        _ = this.Database.Seed(this.BuildCard());

        await this.HandleAsync(130);

        _ = Ok<GetActivityContentsApiResponse>(this.m_Presenter).Contents
            .Select(c => c.Content).Should().Equal(
                ["Get cleaning broom", "Get cleaning material"],
                "an unprojected collection would quietly present the section as empty");
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
