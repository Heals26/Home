using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeIngredients.SetRecipeIngredientSequence;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeIngredients.SetRecipeIngredientSequence;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeIngredients.SetRecipeIngredientSequence;

/// <summary>
/// Moving an ingredient up or down a recipe. Half of the two-call swap the reorder control makes,
/// so the same ingredient in another recipe must not move with it.
/// </summary>
public class SetRecipeIngredientSequenceInteractorTests : InteractorTest
{

    #region Fields

    private readonly SetRecipeIngredientSequencePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, params (long IngredientID, long Sequence)[] ingredients)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        _Recipe.Ingredients =
        [
            .. ingredients.Select(i => new RecipeIngredient()
            {
                Ingredient = new Ingredient() { IngredientID = i.IngredientID, Name = $"Ingredient {i.IngredientID}" },
                Recipe = _Recipe,
                Sequence = i.Sequence
            })
        ];

        return _Recipe;
    }

    private Task HandleAsync(long recipeID, long ingredientID, long sequence)
        => new SetRecipeIngredientSequenceInteractor().HandleAsync(
            new SetRecipeIngredientSequenceInputPort(ingredientID, recipeID, sequence),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_MovesTheIngredientAndSavesIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, (130, 1), (131, 2)));

        await this.HandleAsync(120, 131, 1);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<RecipeIngredient>().Single(ri => ri.IngredientID == 131).Sequence.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_LeavesTheOtherIngredientsWhereTheyAre()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, (130, 1), (131, 2)));

        await this.HandleAsync(120, 131, 1);

        _ = this.Stored<RecipeIngredient>().Single(ri => ri.IngredientID == 130).Sequence.Should().Be(
            1,
            "the caller makes the swap in two calls, so this one moves exactly what it was asked to");
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, 1)),
            BuildRecipe(920, this.Theirs, (930, 1)));

        await this.HandleAsync(920, 930, 9);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<RecipeIngredient>().Single(ri => ri.IngredientID == 930).Sequence.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheIngredientIsNotInThatRecipe_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, 1)),
            BuildRecipe(121, this.Ours, (131, 1)));

        await this.HandleAsync(120, 131, 5);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<RecipeIngredient>().Single(ri => ri.IngredientID == 131).Sequence.Should().Be(1);
    }

    #endregion Methods

}
