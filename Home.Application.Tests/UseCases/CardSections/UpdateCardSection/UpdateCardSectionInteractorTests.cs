using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.CardSections.UpdateCardSection;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CardSections.UpdateCardSection;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.CardSections.UpdateCardSection;

/// <summary>
/// Renaming a heading or moving it up the card. Both travel through
/// <see cref="PropertyChangeTracker{TProperty}"/>, so setting one must leave the other alone: that
/// is the whole reason the tracker exists rather than a plain nullable.
/// </summary>
public class UpdateCardSectionInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateCardSectionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static CardSection BuildSection(long cardSectionID, Household household, string name, int sequence)
        => new()
        {
            CardSectionID = cardSectionID,
            Household = household,
            Name = name,
            Sequence = sequence
        };

    private Task HandleAsync(long cardSectionID, PropertyChangeTracker<string> name = default, PropertyChangeTracker<int> sequence = default)
        => new UpdateCardSectionInteractor().HandleAsync(
            new UpdateCardSectionInputPort(cardSectionID, name, sequence),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RenamesTheSectionAndSavesIt()
    {
        _ = this.Database.Seed(BuildSection(110, this.Ours, "Details", 1));

        await this.HandleAsync(110, name: new("Notes"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<CardSection>().Single().Name.Should().Be("Notes");
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheNameIsSent_LeavesTheSequenceAlone()
    {
        _ = this.Database.Seed(BuildSection(110, this.Ours, "Details", 3));

        await this.HandleAsync(110, name: new("Notes"));

        _ = this.Stored<CardSection>().Single().Sequence.Should().Be(
            3,
            "an untouched property must survive, which is the point of the change tracker");
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheSequenceIsSent_LeavesTheNameAlone()
    {
        _ = this.Database.Seed(BuildSection(110, this.Ours, "Details", 3));

        await this.HandleAsync(110, sequence: new(0));

        var _Stored = this.Stored<CardSection>().Single();

        _ = _Stored.Sequence.Should().Be(0);
        _ = _Stored.Name.Should().Be("Details");
    }

    [Fact]
    public async Task HandleAsync_WhenTheSectionBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(
            BuildSection(110, this.Ours, "Details", 1),
            BuildSection(910, this.Theirs, "Theirs", 1));

        await this.HandleAsync(910, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<CardSection>().Single(s => s.CardSectionID == 910).Name.Should().Be("Theirs");
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchSectionExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildSection(110, this.Ours, "Details", 1));

        await this.HandleAsync(404, name: new("Notes"));

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
