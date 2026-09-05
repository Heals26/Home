using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Tags.UpdateTag;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Tags.UpdateTag;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Tags.UpdateTag;

/// <summary>
/// Renaming or recolouring a label. The name clash check has to ignore the tag being renamed, or
/// saving a tag under its own name would refuse itself.
/// </summary>
public class UpdateTagInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateTagPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Tag BuildTag(long tagID, Household household, string name, string colour = "#FF0000")
        => new()
        {
            Activities = [],
            Colour = colour,
            Household = household,
            Name = name,
            TagID = tagID
        };

    private Task HandleAsync(long tagID, PropertyChangeTracker<string> colour = default, PropertyChangeTracker<string> name = default)
        => new UpdateTagInteractor().HandleAsync(
            new UpdateTagInputPort(tagID, colour, name),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RenamesTheTagAndSavesIt()
    {
        _ = this.Database.Seed(BuildTag(150, this.Ours, "Chores"));

        await this.HandleAsync(150, name: new("  Housework  "));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Tag>().Single().Name.Should().Be("Housework");
    }

    [Fact]
    public async Task HandleAsync_StoresANewColourInOneCase()
    {
        _ = this.Database.Seed(BuildTag(150, this.Ours, "Chores"));

        await this.HandleAsync(150, colour: new("#00ffaa"));

        _ = this.Stored<Tag>().Single().Colour.Should().Be("#00FFAA");
    }

    [Fact]
    public async Task HandleAsync_SavingATagUnderItsOwnNameIsNotAClash()
    {
        _ = this.Database.Seed(BuildTag(150, this.Ours, "Chores"));

        await this.HandleAsync(150, name: new("Chores"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherTagAlreadyHasThatName_RefusesAndChangesNothing()
    {
        _ = this.Database.Seed(BuildTag(150, this.Ours, "Chores"), BuildTag(151, this.Ours, "Outside"));

        await this.HandleAsync(151, name: new("Chores"));

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<Tag>().Single(t => t.TagID == 151).Name.Should().Be("Outside");
    }

    [Fact]
    public async Task HandleAsync_WhenTheClashingNameIsAnotherHouseholds_AllowsIt()
    {
        _ = this.Database.Seed(BuildTag(150, this.Ours, "Outside"), BuildTag(950, this.Theirs, "Chores"));

        await this.HandleAsync(150, name: new("Chores"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Tag>().Single(t => t.TagID == 150).Name.Should().Be("Chores");
    }

    [Fact]
    public async Task HandleAsync_WhenTheTagBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(BuildTag(950, this.Theirs, "Theirs"));

        await this.HandleAsync(950, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Tag>().Single().Name.Should().Be("Theirs");
    }

    #endregion Methods

}
