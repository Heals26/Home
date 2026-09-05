using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityRegions.UpdateActivityRegion;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ActivityRegions.UpdateActivityRegion;

/// <summary>
/// Moving a section up or down one card. The household check and the write happen in two different
/// places: the interactor proves ownership, then <c>ActivityLogic</c> looks the region up again by
/// ID alone. That split is where an isolation hole could open, so it is what these pin.
/// </summary>
public class UpdateActivityRegionInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateActivityRegionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, Household household, params long[] regionIDs)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = household, Title = $"Card {activityID}" };

        _Activity.Regions =
        [
            .. regionIDs.Select((id, index) => new ActivityRegion()
            {
                ActivityRegionID = id,
                Activity = _Activity,
                CardSection = new CardSection() { CardSectionID = id + 1000, Household = household, Name = $"Section {id}", Sequence = index },
                Sequence = index
            })
        ];

        return _Activity;
    }

    private Task HandleAsync(long activityRegionID, PropertyChangeTracker<int> sequence = default)
    {
        var _Services = this.Services(out var _Context);

        return new UpdateActivityRegionInteractor().HandleAsync(
            new UpdateActivityRegionInputPort(activityRegionID, sequence),
            this.m_Presenter,
            _Services.With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_MovesTheSectionAndSavesIt()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130, 131));

        await this.HandleAsync(131, sequence: new(0));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ActivityRegion>().Single(r => r.ActivityRegionID == 131).Sequence.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSequenceIsSent_ChangesNothing()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130, 131));

        await this.HandleAsync(131);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ActivityRegion>().Single(r => r.ActivityRegionID == 131).Sequence.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRegionBelongsToAnotherHousehold_RefusesBeforeReachingTheWrite()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, this.Ours, 130),
            this.BuildCard(900, this.Theirs, 930));

        await this.HandleAsync(930, sequence: new(9));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityRegion>().Single(r => r.ActivityRegionID == 930).Sequence.Should().Be(
            0,
            "the ownership check is the only thing standing between us and another card, because the write looks up by ID alone");
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchRegionExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130));

        await this.HandleAsync(404, sequence: new(0));

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
