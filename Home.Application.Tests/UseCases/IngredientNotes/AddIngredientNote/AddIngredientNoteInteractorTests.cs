using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.IngredientNotes.AddIngredientNote;
using Home.Domain.Entities;
using Home.WebApi.Presenters.IngredientNotes.AddIngredientNote;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.IngredientNotes.AddIngredientNote;

/// <summary>
/// Pinning a note to an ingredient, which is the "the good olive oil" case. Nothing in the app
/// reaches this yet, so these tests are what keeps the slice honest until something does.
/// </summary>
public class AddIngredientNoteInteractorTests : InteractorTest
{

    #region Fields

    private readonly AddIngredientNotePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, Ingredient ingredient)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        _Recipe.Ingredients = [new RecipeIngredient() { Ingredient = ingredient, Recipe = _Recipe, Sequence = 1 }];

        return _Recipe;
    }

    private Task HandleAsync(long ingredientID, string content)
        => new AddIngredientNoteInteractor().HandleAsync(
            new AddIngredientNoteInputPort(content, ingredientID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheNoteAndJoinsItToTheIngredient()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, new Ingredient() { IngredientID = 130, Name = "Olive oil", Notes = [] }));

        await this.HandleAsync(130, "The good one");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Note = this.Stored<Note>().Single();

        _ = _Note.Content.Should().Be("The good one");
        _ = _Note.CreatedOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
        _ = this.Stored<IngredientNote>().Count(n => n.IngredientID == 130 && n.NoteID == _Note.NoteID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheIngredientIsOnlyInAnotherHouseholdsRecipe_PresentsNotFoundAndWritesNothing()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, new Ingredient() { IngredientID = 130, Name = "Olive oil", Notes = [] }),
            BuildRecipe(920, this.Theirs, new Ingredient() { IngredientID = 930, Name = "Truffle oil", Notes = [] }));

        await this.HandleAsync(930, "Written by us");

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchIngredientExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, new Ingredient() { IngredientID = 130, Name = "Olive oil", Notes = [] }));

        await this.HandleAsync(404, "The good one");

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Should().BeEmpty();
    }

    #endregion Methods

}
