using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeNotes.AddRecipeNote;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeNotes.AddRecipeNote;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeNotes.AddRecipeNote;

/// <summary>
/// Pinning a note to a recipe. The note is written first and joined second, because the join row
/// needs an ID that only the database can give it.
/// </summary>
public class AddRecipeNoteInteractorTests : InteractorTest
{

    #region Fields

    private readonly AddRecipeNotePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household)
        => new()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            Notes = [],
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

    private Task HandleAsync(long recipeID, string content)
        => new AddRecipeNoteInteractor().HandleAsync(
            new AddRecipeNoteInputPort(content, recipeID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheNoteAndJoinsItToTheRecipe()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, "Freezes well");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Note = this.Stored<Note>().Single();

        _ = _Note.Content.Should().Be("Freezes well");
        _ = this.Stored<RecipeNote>().Count(rn => rn.RecipeID == 120 && rn.NoteID == _Note.NoteID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_StampsTheNoteWithTheClockRatherThanReadingItDirectly()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, "Freezes well");

        _ = this.Stored<Note>().Single().CreatedOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFoundAndWritesNothing()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours), BuildRecipe(920, this.Theirs));

        await this.HandleAsync(920, "Written by us");

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Should().BeEmpty("the note must not be left behind when the join is refused");
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchRecipeExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(404, "Freezes well");

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Should().BeEmpty();
    }

    #endregion Methods

}
