using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.CardSections.CreateCardSection;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CardSections.CreateCardSection;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.CardSections.CreateCardSection;

/// <summary>
/// Adding a heading to the household's cards. The sequence is worked out from what is already
/// there rather than sent by the caller, so two people adding at once cannot land on the same
/// position.
/// </summary>
public class CreateCardSectionInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateCardSectionPresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(string name)
        => new CreateCardSectionInteractor().HandleAsync(
            new CreateCardSectionInputPort(name),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheSectionToTheSignedInHousehold()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Shopping");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        _ = this.Stored<CardSection>().Single().Name.Should().Be("Shopping");
        _ = this.Stored<CardSection>().Count(s => s.Household.HouseholdID == OurHouseholdID).Should().Be(
            1,
            "the section is attached to the household that asked for it, not left dangling");
    }

    [Fact]
    public async Task HandleAsync_PutsANewSectionOnTheEnd()
    {
        _ = this.Database.Seed(
            BuildSection(110, this.Ours, "Details", 0),
            BuildSection(111, this.Ours, "Steps", 1));

        await this.HandleAsync("Notes");

        _ = this.Stored<CardSection>().Single(s => s.Name == "Notes").Sequence.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_OnAHouseholdWithNoSectionsStartsAtZero()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Details");

        _ = this.Stored<CardSection>().Single().Sequence.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_CountsOnlyOurSectionsWhenWorkingOutTheEnd()
    {
        _ = this.Database.Seed(
            BuildSection(110, this.Ours, "Details", 0),
            BuildSection(910, this.Theirs, "Theirs", 47));

        await this.HandleAsync("Steps");

        _ = this.Stored<CardSection>().Single(s => s.Name == "Steps").Sequence.Should().Be(
            1,
            "another household's numbering has nothing to do with ours");
    }

    #endregion Methods

}
