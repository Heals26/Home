using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityContents.UpdateActivityContent;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityContents.UpdateActivityContent;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ActivityContents.UpdateActivityContent;

/// <summary>
/// Editing a line on a card. Like the region update, the ownership check and the write are in two
/// different places and only the first one knows about households.
/// </summary>
public class UpdateActivityContentInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateActivityContentPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, Household household, long contentID, string content, int sequence)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = household, Title = $"Card {activityID}" };

        var _Region = new ActivityRegion()
        {
            ActivityRegionID = activityID + 30,
            Activity = _Activity,
            CardSection = new CardSection() { CardSectionID = activityID + 1000, Household = household, Name = "Details", Sequence = 1 },
            Sequence = 1
        };

        _Region.Fields = [new ActivityContent() { ActivityContentID = contentID, Content = content, Region = _Region, Sequence = sequence }];
        _Activity.Regions = [_Region];

        return _Activity;
    }

    private Task HandleAsync(long activityContentID, PropertyChangeTracker<string> content = default, PropertyChangeTracker<int> sequence = default)
    {
        var _Services = this.Services(out var _Context);

        return new UpdateActivityContentInteractor().HandleAsync(
            new UpdateActivityContentInputPort(activityContentID, content, sequence),
            this.m_Presenter,
            _Services.With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_RewritesTheLineAndSavesIt()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 140, "Get broom", 1));

        await this.HandleAsync(140, content: new("Get the good broom"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ActivityContent>().Single().Content.Should().Be("Get the good broom");
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheContentIsSent_LeavesThePositionAlone()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 140, "Get broom", 4));

        await this.HandleAsync(140, content: new("Get the good broom"));

        _ = this.Stored<ActivityContent>().Single().Sequence.Should().Be(4);
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyThePositionIsSent_LeavesTheContentAlone()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 140, "Get broom", 4));

        await this.HandleAsync(140, sequence: new(1));

        var _Stored = this.Stored<ActivityContent>().Single();

        _ = _Stored.Sequence.Should().Be(1);
        _ = _Stored.Content.Should().Be("Get broom");
    }

    [Fact]
    public async Task HandleAsync_WhenTheLineBelongsToAnotherHousehold_RefusesBeforeReachingTheWrite()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, this.Ours, 140, "Ours", 1),
            this.BuildCard(900, this.Theirs, 940, "Theirs", 1));

        await this.HandleAsync(940, content: new("Rewritten by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityContent>().Single(c => c.ActivityContentID == 940).Content.Should().Be("Theirs");
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchLineExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 140, "Get broom", 1));

        await this.HandleAsync(404, content: new("Anything"));

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
