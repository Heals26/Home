using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.IngredientNotes.RemoveIngredientNote;
using Home.Domain.Entities;
using Home.WebApi.Presenters.IngredientNotes.RemoveIngredientNote;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.IngredientNotes.RemoveIngredientNote;

/// <summary>
/// Taking a note off an ingredient. The note goes with the join, the same as on a recipe.
/// </summary>
public class RemoveIngredientNoteInteractorTests : InteractorTest
{

    #region Fields

    private readonly RemoveIngredientNotePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, long ingredientID, params long[] noteIDs)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        var _Ingredient = new Ingredient() { IngredientID = ingredientID, Name = "Olive oil" };

        _Ingredient.Notes =
        [
            .. noteIDs.Select(id => new IngredientNote()
            {
                Ingredient = _Ingredient,
                Note = new Note() { Content = $"Note {id}", CreatedOnUTC = new DateTime(2026, 8, 12), NoteID = id }
            })
        ];

        _Recipe.Ingredients = [new RecipeIngredient() { Ingredient = _Ingredient, Recipe = _Recipe, Sequence = 1 }];

        return _Recipe;
    }

    private Task HandleAsync(long ingredientID, long noteID)
        => new RemoveIngredientNoteInteractor().HandleAsync(
            new RemoveIngredientNoteInputPort(ingredientID, noteID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheJoinAndTheNoteBehindIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, 130, 140));

        await this.HandleAsync(130, 140);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<IngredientNote>().Should().BeEmpty();
        _ = this.Stored<Note>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_LeavesTheIngredientAndItsOtherNotesAlone()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, 130, 140, 141));

        await this.HandleAsync(130, 140);

        _ = this.Stored<Note>().Select(n => n.NoteID).Should().Equal([141]);
        _ = this.Stored<Ingredient>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheNoteIsOnAnotherHouseholdsIngredient_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, 130, 140),
            BuildRecipe(920, this.Theirs, 930, 940));

        await this.HandleAsync(930, 940);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenTheNoteIsOnADifferentIngredient_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, 130, 140),
            BuildRecipe(121, this.Ours, 131, 141));

        await this.HandleAsync(130, 141);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Should().HaveCount(2);
    }

    #endregion Methods

}
