using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Tags.CreateTag;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Tags.CreateTag;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Tags.CreateTag;

/// <summary>
/// Adding a label. The name is unique per household in the database, so the clash is caught here
/// and answered rather than left to fail as a save.
/// </summary>
public class CreateTagInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateTagPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Tag BuildTag(long tagID, Household household, string name)
        => new()
        {
            Activities = [],
            Colour = "#FF0000",
            Household = household,
            Name = name,
            TagID = tagID
        };

    private Task HandleAsync(string name, string colour = "#ff0000")
        => new CreateTagInteractor().HandleAsync(
            new CreateTagInputPort(colour, name),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheTagToTheSignedInHousehold()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Chores");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<Tag>().Single();

        _ = _Stored.Name.Should().Be("Chores");
        _ = this.Stored<Tag>().Count(t => t.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_StoresTheColourInOneCaseSoTwoSpellingsOfTheSameColourMatch()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Chores", "#ff00aa");

        _ = this.Stored<Tag>().Single().Colour.Should().Be("#FF00AA");
    }

    [Fact]
    public async Task HandleAsync_TrimsTheName()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("  Chores  ");

        _ = this.Stored<Tag>().Single().Name.Should().Be("Chores");
    }

    [Fact]
    public async Task HandleAsync_WhenTheHouseholdAlreadyHasThatName_RefusesRatherThanFailingTheSave()
    {
        _ = this.Database.Seed(BuildTag(150, this.Ours, "Chores"));

        await this.HandleAsync("Chores");

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<Tag>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_AllowsANameAnotherHouseholdAlreadyUses()
    {
        _ = this.Database.Seed(BuildTag(950, this.Theirs, "Chores"));

        await this.HandleAsync("Chores");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
        _ = this.Stored<Tag>().Should().HaveCount(2, "names are unique inside a household, not across them");
    }

    #endregion Methods

}
