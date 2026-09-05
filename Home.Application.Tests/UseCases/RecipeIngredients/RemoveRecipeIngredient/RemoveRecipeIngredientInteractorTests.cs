using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeIngredients.RemoveRecipeIngredient;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeIngredients.RemoveRecipeIngredient;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeIngredients.RemoveRecipeIngredient;

/// <summary>
/// Taking an ingredient out of a recipe. The ingredient row goes with the join, because ingredient
/// rows are never shared between recipes here: each recipe writes its own.
/// </summary>
public class RemoveRecipeIngredientInteractorTests : InteractorTest
{

    #region Fields

    private readonly RemoveRecipeIngredientPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, params (long IngredientID, string Name)[] ingredients)
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
            .. ingredients.Select((i, index) => new RecipeIngredient()
            {
                Ingredient = new Ingredient() { IngredientID = i.IngredientID, Name = i.Name },
                Recipe = _Recipe,
                Sequence = index + 1
            })
        ];

        return _Recipe;
    }

    private Task HandleAsync(long recipeID, long ingredientID)
        => new RemoveRecipeIngredientInteractor().HandleAsync(
            new RemoveRecipeIngredientInputPort(ingredientID, recipeID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheJoinAndTheIngredientBehindIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, (130, "Onion")));

        await this.HandleAsync(120, 130);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<RecipeIngredient>().Should().BeEmpty();
        _ = this.Stored<Ingredient>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_LeavesTheOtherIngredientsAndTheRecipeAlone()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, (130, "Onion"), (131, "Garlic")));

        await this.HandleAsync(120, 130);

        _ = this.Stored<Ingredient>().Select(i => i.Name).Should().Equal(["Garlic"]);
        _ = this.Stored<Recipe>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheIngredientIsOnAnotherHouseholdsRecipe_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "Onion")),
            BuildRecipe(920, this.Theirs, (930, "Truffle")));

        await this.HandleAsync(920, 930);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Ingredient>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenTheIngredientIsOnADifferentRecipeOfOurs_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "Onion")),
            BuildRecipe(121, this.Ours, (131, "Garlic")));

        await this.HandleAsync(120, 131);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Ingredient>().Should().HaveCount(2);
    }

    #endregion Methods

}
