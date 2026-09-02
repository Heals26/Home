using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.GetActivity;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Activities.GetActivity;
using Home.WebApi.UseCases.Activities.GetActivity;

namespace Home.Application.Tests.UseCases.Activities.GetActivity;

/// <summary>
/// The card screen, and the regression that started this file. On 1 Sep the presenter began
/// reading a section's name off every region while the interactor still projected only the region
/// and its lines, so every card in the household failed to open.
/// </summary>
public class GetActivityInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetActivityPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildOurCard()
    {
        var _Details = new CardSection() { CardSectionID = 110, Household = this.Ours, Name = "Details", Sequence = 1 };
        var _Steps = new CardSection() { CardSectionID = 111, Household = this.Ours, Name = "Steps", Sequence = 2 };

        var _Activity = new Activity()
        {
            ActivityID = 100,
            Household = this.Ours,
            Sequence = 1,
            State = new ActivityState() { ActivityStateID = 120, Household = this.Ours, Name = "Doing", Sequence = 1 },
            Title = "Clean the balcony",
            User = this.Member
        };

        // Deliberately out of order, so the presenter is the one putting them right.
        var _StepsRegion = new ActivityRegion() { ActivityRegionID = 131, Activity = _Activity, CardSection = _Steps, Sequence = 2 };
        var _DetailsRegion = new ActivityRegion() { ActivityRegionID = 130, Activity = _Activity, CardSection = _Details, Sequence = 1 };

        _Activity.Regions = [_StepsRegion, _DetailsRegion];
        _DetailsRegion.Fields =
        [
            new ActivityContent() { ActivityContentID = 141, Content = "Get cleaning material", Region = _DetailsRegion, Sequence = 2 },
            new ActivityContent() { ActivityContentID = 140, Content = "Get cleaning broom", Region = _DetailsRegion, Sequence = 1 }
        ];

        _Activity.Tags =
        [
            new ActivityTag() { Activity = _Activity, Tag = new Tag() { TagID = 151, Colour = "#00FF00", Household = this.Ours, Name = "Outside" } },
            new ActivityTag() { Activity = _Activity, Tag = new Tag() { TagID = 150, Colour = "#FF0000", Household = this.Ours, Name = "Chores" } }
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
        => new GetActivityInteractor().HandleAsync(
            new GetActivityInputPort(activityID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WhenTheCardExists_NamesTheSectionEveryRegionSitsUnder()
    {
        _ = this.Database.Seed(this.BuildOurCard());

        await this.HandleAsync(100);

        var _Response = Ok<GetActivityApiResponse>(this.m_Presenter);

        _ = _Response.Regions.Select(r => r.CardSectionName).Should().Equal(
            ["Details", "Steps"],
            "the presenter reads the section off every region, so the query has to load it");
        _ = _Response.Regions.Select(r => r.CardSectionID).Should().Equal(110, 111);
    }

    [Fact]
    public async Task HandleAsync_OrdersRegionsAndTheirLinesBySequence()
    {
        _ = this.Database.Seed(this.BuildOurCard());

        await this.HandleAsync(100);

        var _Response = Ok<GetActivityApiResponse>(this.m_Presenter);

        _ = _Response.Regions.Select(r => r.ActivityRegionID).Should().Equal(130, 131);
        _ = _Response.Regions[0].Fields.Select(f => f.Content).Should()
            .Equal("Get cleaning broom", "Get cleaning material");
    }

    [Fact]
    public async Task HandleAsync_BringsBackTheColumnAssigneeAndTags()
    {
        _ = this.Database.Seed(this.BuildOurCard());

        await this.HandleAsync(100);

        var _Response = Ok<GetActivityApiResponse>(this.m_Presenter);

        _ = _Response.Title.Should().Be("Clean the balcony");
        _ = _Response.StateID.Should().Be(120);
        _ = _Response.State.Should().Be("Doing");
        _ = _Response.AssignedToUserID.Should().Be(this.Member.UserID);
        _ = _Response.AssignedTo.Should().Be(this.Member.UserName);
        _ = _Response.Tags.Select(t => t.Name).Should().Equal("Chores", "Outside");
    }

    [Fact]
    public async Task HandleAsync_WhenTheCardBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildOurCard(), this.BuildTheirCard());

        await this.HandleAsync(900);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchCardExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildOurCard());

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
