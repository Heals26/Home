using FluentAssertions;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Services.EntityLogic.Recipes;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.AddRecipeToShoppingList;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingLists.AddRecipeToShoppingList;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingLists.AddRecipeToShoppingList;

/// <summary>
/// Putting a recipe's ingredients on a shopping list. Amounts of the same thing measured the same
/// way are added together; measured differently they stay as two lines, because two cups and two
/// hundred grams of the same thing are not 202 of anything.
/// </summary>
public class AddRecipeToShoppingListInteractorTests : InteractorTest
{

    #region Fields

    private readonly AddRecipeToShoppingListPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, params (long IngredientID, string Name, decimal? Amount, long? Unit)[] ingredients)
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
                Ingredient = new Ingredient() { Amount = i.Amount, IngredientID = i.IngredientID, Name = i.Name, Unit = i.Unit },
                Recipe = _Recipe,
                Sequence = index + 1
            })
        ];

        return _Recipe;
    }

    private static ShoppingList BuildList(long shoppingListID, Household household, params (long ItemID, string Name, decimal? Amount, long? Unit)[] items)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            .. items.Select((i, index) => new ShoppingListItem()
            {
                Amount = i.Amount,
                Name = i.Name,
                Sequence = index + 1,
                ShoppingList = _List,
                ShoppingListItemID = i.ItemID,
                Unit = i.Unit
            })
        ];

        return _List;
    }

    private Task HandleAsync(long recipeID, long shoppingListID, IReadOnlyList<long>? ingredientIDs = null)
    {
        var _Services = this.Services(out var _Context);

        return new AddRecipeToShoppingListInteractor().HandleAsync(
            new AddRecipeToShoppingListInputPort(ingredientIDs, recipeID, shoppingListID),
            this.m_Presenter,
            _Services.With<IRecipeLogic>(new RecipeLogic()).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_AddsEveryIngredientWhenNoneAreNamed()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "Onion", 2, MeasurementUnitSE.Pieces.Value), (131, "Beef mince", 500, MeasurementUnitSE.Grams.Value)),
            BuildList(140, this.Ours));

        await this.HandleAsync(120, 140);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Select(i => i.Name).Should().BeEquivalentTo(["Onion", "Beef mince"]);
    }

    [Fact]
    public async Task HandleAsync_AddsOnlyTheIngredientsTheFamilyTicked()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "Onion", 2, MeasurementUnitSE.Pieces.Value), (131, "Beef mince", 500, MeasurementUnitSE.Grams.Value)),
            BuildList(140, this.Ours));

        await this.HandleAsync(120, 140, [131]);

        _ = this.Stored<ShoppingListItem>().Select(i => i.Name).Should().Equal(["Beef mince"]);
    }

    [Fact]
    public async Task HandleAsync_AddsToALineAlreadyOnTheListWhenTheUnitMatches()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "Onion", 2, MeasurementUnitSE.Pieces.Value)),
            BuildList(140, this.Ours, (150, "Onion", 1, MeasurementUnitSE.Pieces.Value)));

        await this.HandleAsync(120, 140);

        _ = this.Stored<ShoppingListItem>().Should().ContainSingle();
        _ = this.Stored<ShoppingListItem>().Single().Amount.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_KeepsTheSameThingMeasuredDifferentlyAsTwoLines()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "Onion", 200, MeasurementUnitSE.Grams.Value)),
            BuildList(140, this.Ours, (150, "Onion", 2, MeasurementUnitSE.Pieces.Value)));

        await this.HandleAsync(120, 140);

        _ = this.Stored<ShoppingListItem>().Should().HaveCount(
            2,
            "two cups and two hundred grams of the same thing are two lines, not 202 of nothing");
    }

    [Fact]
    public async Task HandleAsync_MatchesAnExistingLineWhateverTheCase()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "onion", 2, MeasurementUnitSE.Pieces.Value)),
            BuildList(140, this.Ours, (150, "Onion", 1, MeasurementUnitSE.Pieces.Value)));

        await this.HandleAsync(120, 140);

        _ = this.Stored<ShoppingListItem>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFoundAndAddsNothing()
    {
        _ = this.Database.Seed(
            BuildRecipe(920, this.Theirs, (930, "Truffle", 1, null)),
            BuildList(140, this.Ours));

        await this.HandleAsync(920, 140);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingListItem>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_PresentsNotFoundAndAddsNothing()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, (130, "Onion", 2, MeasurementUnitSE.Pieces.Value)),
            BuildList(940, this.Theirs));

        await this.HandleAsync(120, 940);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingListItem>().Should().BeEmpty();
    }

    #endregion Methods

}
