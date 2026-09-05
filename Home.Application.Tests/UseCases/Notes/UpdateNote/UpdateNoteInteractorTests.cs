using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Notes.UpdateNote;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Notes.UpdateNote;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Notes.UpdateNote;

/// <summary>
/// Editing a note. A <c>Note</c> carries no household of its own, so this slice has to find one by
/// looking down both of the paths a note can be pinned along: a recipe, or an ingredient inside a
/// recipe. Miss either and the household can no longer edit half its own notes.
/// </summary>
public class UpdateNoteInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateNotePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Note BuildNote(long noteID, string content)
        => new()
        {
            Content = content,
            CreatedOnUTC = new DateTime(2026, 8, 12),
            NoteID = noteID
        };

    private static Recipe BuildRecipeWithNote(long recipeID, Household household, Note note)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        _Recipe.Notes = [new RecipeNote() { Note = note, Recipe = _Recipe }];

        return _Recipe;
    }

    private static Recipe BuildRecipeWithIngredientNote(long recipeID, Household household, Note note)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        var _Ingredient = new Ingredient() { IngredientID = recipeID + 10, Name = "Olive oil" };
        _Ingredient.Notes = [new IngredientNote() { Ingredient = _Ingredient, Note = note }];
        _Recipe.Ingredients = [new RecipeIngredient() { Ingredient = _Ingredient, Recipe = _Recipe, Sequence = 1 }];

        return _Recipe;
    }

    private Task HandleAsync(long noteID, PropertyChangeTracker<string> content = default)
        => new UpdateNoteInteractor().HandleAsync(
            new UpdateNoteInputPort(noteID, content),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RewritesANotePinnedToOneOfOurRecipes()
    {
        _ = this.Database.Seed(BuildRecipeWithNote(120, this.Ours, BuildNote(140, "Freezes well")));

        await this.HandleAsync(140, content: new("Freezes for three months"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Note>().Single().Content.Should().Be("Freezes for three months");
    }

    [Fact]
    public async Task HandleAsync_RewritesANotePinnedToAnIngredientInOneOfOurRecipes()
    {
        _ = this.Database.Seed(BuildRecipeWithIngredientNote(120, this.Ours, BuildNote(140, "The good one")));

        await this.HandleAsync(140, content: new("Woolies brand only"));

        _ = this.Stored<Note>().Single().Content.Should().Be(
            "Woolies brand only",
            "an ingredient note is reachable through the recipes using the ingredient, not directly");
    }

    [Fact]
    public async Task HandleAsync_WhenNoContentIsSent_LeavesTheNoteAlone()
    {
        _ = this.Database.Seed(BuildRecipeWithNote(120, this.Ours, BuildNote(140, "Freezes well")));

        await this.HandleAsync(140);

        _ = this.Stored<Note>().Single().Content.Should().Be("Freezes well");
    }

    [Fact]
    public async Task HandleAsync_WhenTheNoteBelongsToAnotherHouseholdsRecipe_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(BuildRecipeWithNote(920, this.Theirs, BuildNote(940, "Theirs")));

        await this.HandleAsync(940, content: new("Rewritten by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Single().Content.Should().Be("Theirs");
    }

    [Fact]
    public async Task HandleAsync_WhenTheNoteBelongsToAnotherHouseholdsIngredient_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipeWithIngredientNote(920, this.Theirs, BuildNote(940, "Theirs")));

        await this.HandleAsync(940, content: new("Rewritten by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Single().Content.Should().Be("Theirs");
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchNoteExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipeWithNote(120, this.Ours, BuildNote(140, "Freezes well")));

        await this.HandleAsync(404, content: new("Anything"));

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
