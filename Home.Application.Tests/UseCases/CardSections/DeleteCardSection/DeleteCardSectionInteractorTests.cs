using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.CardSections.DeleteCardSection;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CardSections.DeleteCardSection;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.CardSections.DeleteCardSection;

/// <summary>
/// Removing a heading, and refusing to when cards are still written under it.
/// <para>
/// The refusal depends on the query projecting the regions. Left unprojected the collection arrives
/// empty, the guard reads zero, and the section goes along with every line of writing on every card
/// using it. There is no undo. This is the same fault that made the settings sheet offer the delete
/// in the first place, one layer down.
/// </para>
/// </summary>
public class DeleteCardSectionInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteCardSectionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static CardSection BuildSection(long cardSectionID, Household household, string name)
        => new()
        {
            CardSectionID = cardSectionID,
            Household = household,
            Name = name,
            Sequence = 1
        };

    private Activity BuildCardUsing(long activityID, CardSection section)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = this.Ours, Title = $"Card {activityID}" };

        _Activity.Regions =
        [
            new ActivityRegion()
            {
                ActivityRegionID = activityID + 100,
                Activity = _Activity,
                CardSection = section,
                Sequence = 1
            }
        ];

        return _Activity;
    }

    private Task HandleAsync(long cardSectionID)
        => new DeleteCardSectionInteractor().HandleAsync(
            new DeleteCardSectionInputPort(cardSectionID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WhenNothingUsesTheSection_DeletesIt()
    {
        _ = this.Database.Seed(BuildSection(110, this.Ours, "Steps"));

        await this.HandleAsync(110);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<CardSection>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenACardIsWrittenUnderIt_RefusesAndKeepsIt()
    {
        var _Details = BuildSection(110, this.Ours, "Details");

        _ = this.Database.Seed(this.BuildCardUsing(100, _Details));

        await this.HandleAsync(110);

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>(
            "the section exists, the household simply cannot have it while cards use it");
        _ = this.Stored<CardSection>().Should().ContainSingle(
            "an unprojected region collection would read as zero and take the writing with it");
        _ = this.Stored<ActivityRegion>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheSectionBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            BuildSection(110, this.Ours, "Details"),
            BuildSection(910, this.Theirs, "Theirs"));

        await this.HandleAsync(910);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<CardSection>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchSectionExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildSection(110, this.Ours, "Details"));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
