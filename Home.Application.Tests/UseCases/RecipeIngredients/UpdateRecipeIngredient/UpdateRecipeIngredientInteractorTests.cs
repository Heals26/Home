using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.RecipeIngredients.UpdateRecipeIngredient;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeIngredients.UpdateRecipeIngredient;

/// <summary>
/// Editing an ingredient. It carries no household of its own, so ownership is proved through the
/// recipes using it.
/// </summary>
public class UpdateRecipeIngredientInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateRecipeIngredientPresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(
        long ingredientID,
        PropertyChangeTracker<decimal?> amount = default,
        PropertyChangeTracker<string> name = default,
        PropertyChangeTracker<long?> unit = default)
        => new UpdateRecipeIngredientInteractor().HandleAsync(
            new UpdateRecipeIngredientInputPort(amount, ingredientID, name, unit),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RewritesTheIngredientAndSavesIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, new Ingredient()
        {
            Amount = 1,
            IngredientID = 130,
            Name = "Onion",
            Unit = MeasurementUnitSE.Pieces.Value
        }));

        await this.HandleAsync(130, amount: new(500), name: new("Brown onion"), unit: new(MeasurementUnitSE.Grams.Value));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();

        var _Stored = this.Stored<Ingredient>().Single();

        _ = _Stored.Amount.Should().Be(500);
        _ = _Stored.Name.Should().Be("Brown onion");
        _ = _Stored.Unit.Should().Be(MeasurementUnitSE.Grams.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheNameIsSent_LeavesTheAmountAndUnitAlone()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, new Ingredient()
        {
            Amount = 2,
            IngredientID = 130,
            Name = "Onion",
            Unit = MeasurementUnitSE.Pieces.Value
        }));

        await this.HandleAsync(130, name: new("Brown onion"));

        var _Stored = this.Stored<Ingredient>().Single();

        _ = _Stored.Amount.Should().Be(2);
        _ = _Stored.Unit.Should().Be(MeasurementUnitSE.Pieces.Value);
    }

    [Fact]
    public async Task HandleAsync_CanClearAnAmountAndUnitForSomethingMeasuredByEye()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, new Ingredient()
        {
            Amount = 2,
            IngredientID = 130,
            Name = "Salt",
            Unit = MeasurementUnitSE.Pinch.Value
        }));

        await this.HandleAsync(130, amount: new(null), unit: new(null));

        var _Stored = this.Stored<Ingredient>().Single();

        _ = _Stored.Amount.Should().BeNull();
        _ = _Stored.Unit.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheIngredientIsOnlyInAnotherHouseholdsRecipe_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, new Ingredient() { IngredientID = 130, Name = "Onion" }),
            BuildRecipe(920, this.Theirs, new Ingredient() { IngredientID = 930, Name = "Truffle" }));

        await this.HandleAsync(930, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Ingredient>().Single(i => i.IngredientID == 930).Name.Should().Be("Truffle");
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchIngredientExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, new Ingredient() { IngredientID = 130, Name = "Onion" }));

        await this.HandleAsync(404, name: new("Anything"));

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
