using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityRegions.DeleteActivityRegion;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityRegions.DeleteActivityRegion;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ActivityRegions.DeleteActivityRegion;

/// <summary>
/// Taking a section off one card.
/// <para>
/// The lines written under it are carried away by the database, not by this code:
/// <c>FK_ActivityContent_ActivityRegion</c> is configured <c>OnDelete(Cascade)</c>. That is not
/// asserted here, because it cannot be. EF only cascades to dependents it has loaded, and this
/// query deliberately loads none, so in the in-memory harness the lines stay behind while in SQL
/// Server they go. Asserting either way would be recording the harness rather than the product.
/// </para>
/// </summary>
public class DeleteActivityRegionInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteActivityRegionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, Household household, long regionID, int lineCount)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = household, Title = $"Card {activityID}" };

        var _Region = new ActivityRegion()
        {
            ActivityRegionID = regionID,
            Activity = _Activity,
            CardSection = new CardSection() { CardSectionID = regionID + 1000, Household = household, Name = "Details", Sequence = 1 },
            Sequence = 1
        };

        _Region.Fields =
        [
            .. Enumerable.Range(1, lineCount).Select(i => new ActivityContent()
            {
                ActivityContentID = regionID + i,
                Content = $"Line {i}",
                Region = _Region,
                Sequence = i
            })
        ];

        _Activity.Regions = [_Region];

        return _Activity;
    }

    private Task HandleAsync(long activityRegionID)
        => new DeleteActivityRegionInteractor().HandleAsync(
            new DeleteActivityRegionInputPort(activityRegionID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheSectionFromTheCard()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130, lineCount: 2));

        await this.HandleAsync(130);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ActivityRegion>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_LeavesTheCardAndItsSectionDefinitionStanding()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130, lineCount: 1));

        await this.HandleAsync(130);

        _ = this.Stored<Activity>().Should().ContainSingle();
        _ = this.Stored<CardSection>().Should().ContainSingle(
            "taking a heading off one card does not remove it from the household");
    }

    [Fact]
    public async Task HandleAsync_WhenTheRegionBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, this.Ours, 130, lineCount: 1),
            this.BuildCard(900, this.Theirs, 930, lineCount: 1));

        await this.HandleAsync(930);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityRegion>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchRegionExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130, lineCount: 1));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
