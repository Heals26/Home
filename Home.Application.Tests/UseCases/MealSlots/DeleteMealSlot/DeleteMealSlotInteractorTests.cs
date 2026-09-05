using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.MealSlots.DeleteMealSlot;
using Home.Domain.Entities;
using Home.WebApi.Presenters.MealSlots.DeleteMealSlot;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.MealSlots.DeleteMealSlot;

/// <summary>
/// Removing a meal from the household's day. Two database shapes force the hand here: a planned
/// meal holds the slot on a restricted key, so that has to be refused rather than attempted, and
/// the recipe links never cascade from this side, so those rows go by hand.
/// </summary>
public class DeleteMealSlotInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteMealSlotPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealSlot BuildSlot(long mealSlotID, Household household, string name)
        => new()
        {
            Household = household,
            MealSlotID = mealSlotID,
            Name = name,
            Recipes = [],
            Sequence = 1
        };

    private static Recipe BuildRecipe(long recipeID, Household household, MealSlot? mealSlot = null)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        if (mealSlot != null)
            _Recipe.MealSlots = [new RecipeMealSlot() { MealSlot = mealSlot, Recipe = _Recipe }];

        return _Recipe;
    }

    private Task HandleAsync(long mealSlotID)
        => new DeleteMealSlotInteractor().HandleAsync(
            new DeleteMealSlotInputPort(mealSlotID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheMealWhenNothingIsPlannedInIt()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Supper"));

        await this.HandleAsync(110);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<MealSlot>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_TakesTheRecipeLinksWithItButLeavesTheRecipes()
    {
        var _Supper = BuildSlot(110, this.Ours, "Supper");

        _ = this.Database.Seed(BuildRecipe(120, this.Ours, _Supper));

        await this.HandleAsync(110);

        _ = this.Stored<RecipeMealSlot>().Should().BeEmpty(
            "the recipe links never cascade from this side, so they go by hand");
        _ = this.Stored<Recipe>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenAMealIsPlannedInIt_RefusesRatherThanFailingTheSave()
    {
        var _Dinner = BuildSlot(110, this.Ours, "Dinner");
        var _Recipe = BuildRecipe(120, this.Ours);

        _ = this.Database.Seed(new MealPlanEntry()
        {
            Date = new DateTime(2026, 8, 12),
            MealPlanEntryID = 150,
            MealSlot = _Dinner,
            Recipe = _Recipe
        });

        await this.HandleAsync(110);

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<MealSlot>().Should().ContainSingle();
        _ = this.Stored<MealPlanEntry>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheMealBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Supper"), BuildSlot(910, this.Theirs, "Theirs"));

        await this.HandleAsync(910);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<MealSlot>().Should().HaveCount(2);
    }

    #endregion Methods

}
