using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.GetRecipes;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Recipes.GetRecipes;
using Home.WebApi.UseCases.Recipes.GetRecipes;

namespace Home.Application.Tests.UseCases.Recipes.GetRecipes;

/// <summary>
/// The recipe book. Its presenter maps through AutoMapper rather than by hand, which moves the
/// unprojected-navigation fault inside the mapper — a recipe whose meal slots were not loaded
/// fails in configuration code rather than anywhere the stack trace points at this query.
/// </summary>
public class GetRecipesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetRecipesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, string name, params MealSlot[] mealSlots)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = name,
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        _Recipe.MealSlots = [.. mealSlots.Select(ms => new RecipeMealSlot() { MealSlot = ms, Recipe = _Recipe })];

        return _Recipe;
    }

    private Task HandleAsync(long? mealSlotID = null)
        => new GetRecipesInteractor().HandleAsync(
            new GetRecipesInputPort(mealSlotID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_NamesTheMealSlotsEachRecipeSuitsInOrderThroughTheDay()
    {
        var _Breakfast = new MealSlot() { Household = this.Ours, MealSlotID = 110, Name = "Breakfast", Sequence = 1 };
        var _Dessert = new MealSlot() { Household = this.Ours, MealSlotID = 112, Name = "Dessert", Sequence = 4 };

        _ = this.Database.Seed(BuildRecipe(120, this.Ours, "Pancakes", _Dessert, _Breakfast));

        await this.HandleAsync();

        _ = Ok<GetRecipesApiResponse>(this.m_Presenter).Recipes.Single()
            .MealSlots.Select(ms => ms.Name).Should().Equal(
                ["Breakfast", "Dessert"],
                "the mapper reads the slot behind each join row, so the query has to load it");
    }

    [Fact]
    public async Task HandleAsync_ReturnsOnlyOurHouseholdsRecipes()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, "Pancakes"),
            BuildRecipe(920, this.Theirs, "Their secret"));

        await this.HandleAsync();

        _ = Ok<GetRecipesApiResponse>(this.m_Presenter).Recipes
            .Select(r => r.Name).Should().Equal(["Pancakes"]);
    }

    [Fact]
    public async Task HandleAsync_WhenAMealSlotIsAskedFor_ReturnsOnlyTheRecipesThatSuitIt()
    {
        var _Breakfast = new MealSlot() { Household = this.Ours, MealSlotID = 110, Name = "Breakfast", Sequence = 1 };
        var _Dinner = new MealSlot() { Household = this.Ours, MealSlotID = 111, Name = "Dinner", Sequence = 3 };

        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, "Pancakes", _Breakfast),
            BuildRecipe(121, this.Ours, "Bolognese", _Dinner),
            BuildRecipe(122, this.Ours, "Toast", _Breakfast));

        await this.HandleAsync(110);

        _ = Ok<GetRecipesApiResponse>(this.m_Presenter).Recipes
            .Select(r => r.Name).Should().BeEquivalentTo("Pancakes", "Toast");
    }

    [Fact]
    public async Task HandleAsync_TurnsAPhotosUploadTimeIntoTheCacheBustingVersion()
    {
        var _Photographed = BuildRecipe(120, this.Ours, "Pancakes");
        _Photographed.ImageUpdatedOnUTC = new DateTime(2026, 8, 12, 9, 30, 0);

        _ = this.Database.Seed(_Photographed, BuildRecipe(121, this.Ours, "Toast"));

        await this.HandleAsync();

        var _Recipes = Ok<GetRecipesApiResponse>(this.m_Presenter).Recipes;

        _ = _Recipes.Single(r => r.Name == "Pancakes").ImageVersion.Should().Be(new DateTime(2026, 8, 12, 9, 30, 0).Ticks);
        _ = _Recipes.Single(r => r.Name == "Toast").ImageVersion.Should().BeNull("no photo means nothing to bust the cache on");
    }

    #endregion Methods

}
