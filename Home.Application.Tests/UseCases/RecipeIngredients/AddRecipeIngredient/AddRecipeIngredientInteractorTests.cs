using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeIngredients.AddRecipeIngredient;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.RecipeIngredients.AddRecipeIngredient;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeIngredients.AddRecipeIngredient;

/// <summary>
/// Adding an ingredient to a recipe. The position lives on the join, not the ingredient, and it is
/// worked out here rather than sent by the caller so two people adding at once cannot land on the
/// same one.
/// </summary>
public class AddRecipeIngredientInteractorTests : InteractorTest
{

    #region Fields

    private readonly AddRecipeIngredientPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, params (long IngredientID, string Name, long Sequence)[] ingredients)
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
                Ingredient = new Ingredient() { IngredientID = i.IngredientID, Name = i.Name },
                Recipe = _Recipe,
                Sequence = i.Sequence
            })
        ];

        return _Recipe;
    }

    private Task HandleAsync(long recipeID, string name, decimal? amount = null, long? unit = null)
        => new AddRecipeIngredientInteractor().HandleAsync(
            new AddRecipeIngredientInputPort(amount, name, recipeID, unit),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheIngredientAndJoinsItToTheRecipe()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, "Beef mince", amount: 500, unit: MeasurementUnitSE.Grams.Value);

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<Ingredient>().Single();

        _ = _Stored.Name.Should().Be("Beef mince");
        _ = _Stored.Amount.Should().Be(500);
        _ = _Stored.Unit.Should().Be(MeasurementUnitSE.Grams.Value);
        _ = this.Stored<RecipeIngredient>().Count(ri => ri.RecipeID == 120 && ri.IngredientID == _Stored.IngredientID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_PutsANewIngredientOnTheEnd()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, (130, "Onion", 1), (131, "Garlic", 2)));

        await this.HandleAsync(120, "Beef mince");

        var _Added = this.Stored<Ingredient>().Single(i => i.Name == "Beef mince");

        _ = this.Stored<RecipeIngredient>().Single(ri => ri.IngredientID == _Added.IngredientID).Sequence.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_OnARecipeWithNoIngredientsStartsAtOne()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, "Beef mince");

        _ = this.Stored<RecipeIngredient>().Single().Sequence.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_CountsOnlyThisRecipesIngredientsWhenWorkingOutTheEnd()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "Onion", 1)),
            BuildRecipe(121, this.Ours, (131, "Garlic", 47)));

        await this.HandleAsync(120, "Beef mince");

        var _Added = this.Stored<Ingredient>().Single(i => i.Name == "Beef mince");

        _ = this.Stored<RecipeIngredient>().Single(ri => ri.IngredientID == _Added.IngredientID).Sequence.Should().Be(
            2,
            "the position belongs to one recipe, not to the ingredient");
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFoundAndWritesNothing()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours), BuildRecipe(920, this.Theirs));

        await this.HandleAsync(920, "Beef mince");

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Ingredient>().Should().BeEmpty();
    }

    #endregion Methods

}
