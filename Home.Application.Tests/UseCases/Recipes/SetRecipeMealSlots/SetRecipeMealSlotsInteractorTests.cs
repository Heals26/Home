using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.SetRecipeMealSlots;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Recipes.SetRecipeMealSlots;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Recipes.SetRecipeMealSlots;

/// <summary>
/// Saying which meals a recipe suits. The caller sends the whole set it wants to be true rather
/// than additions and removals, the same shape as setting a card's tags.
/// </summary>
public class SetRecipeMealSlotsInteractorTests : InteractorTest
{

    #region Fields

    private readonly SetRecipeMealSlotsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealSlot BuildSlot(long mealSlotID, Household household, string name)
        => new()
        {
            Household = household,
            MealSlotID = mealSlotID,
            Name = name,
            Sequence = 1
        };

    private static Recipe BuildRecipe(long recipeID, Household household, params MealSlot[] mealSlots)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        _Recipe.MealSlots = [.. mealSlots.Select(ms => new RecipeMealSlot() { MealSlot = ms, Recipe = _Recipe })];

        return _Recipe;
    }

    private Task HandleAsync(long recipeID, params long[] mealSlotIDs)
        => new SetRecipeMealSlotsInteractor().HandleAsync(
            new SetRecipeMealSlotsInputPort(mealSlotIDs, recipeID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_AddsTheSlotsTheRecipeDidNotHave()
    {
        var _Breakfast = BuildSlot(110, this.Ours, "Breakfast");

        _ = this.Database.Seed(BuildSlot(111, this.Ours, "Dessert"), BuildRecipe(120, this.Ours, _Breakfast));

        await this.HandleAsync(120, 110, 111);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<RecipeMealSlot>().Select(rms => rms.MealSlotID).Should().BeEquivalentTo([110L, 111L]);
    }

    [Fact]
    public async Task HandleAsync_RemovesTheSlotsLeftOutOfTheSet()
    {
        var _Breakfast = BuildSlot(110, this.Ours, "Breakfast");
        var _Dessert = BuildSlot(111, this.Ours, "Dessert");

        _ = this.Database.Seed(BuildRecipe(120, this.Ours, _Breakfast, _Dessert));

        await this.HandleAsync(120, 110);

        _ = this.Stored<RecipeMealSlot>().Select(rms => rms.MealSlotID).Should().Equal([110L]);
    }

    [Fact]
    public async Task HandleAsync_WithAnEmptySetTakesTheRecipeOutOfEveryMeal()
    {
        var _Breakfast = BuildSlot(110, this.Ours, "Breakfast");

        _ = this.Database.Seed(BuildRecipe(120, this.Ours, _Breakfast));

        await this.HandleAsync(120);

        _ = this.Stored<RecipeMealSlot>().Should().BeEmpty();
        _ = this.Stored<MealSlot>().Should().ContainSingle("the meal itself is not deleted with the join");
    }

    [Fact]
    public async Task HandleAsync_WhenASlotBelongsToAnotherHousehold_RefusesTheWholeSet()
    {
        _ = this.Database.Seed(
            BuildSlot(110, this.Ours, "Breakfast"),
            BuildSlot(910, this.Theirs, "Theirs"),
            BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, 110, 910);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<RecipeMealSlot>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Breakfast"), BuildRecipe(920, this.Theirs));

        await this.HandleAsync(920, 110);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<RecipeMealSlot>().Should().BeEmpty();
    }

    #endregion Methods

}
