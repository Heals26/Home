using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.GetRecipe;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Recipes.GetRecipe;
using Home.WebApi.UseCases.Recipes.GetRecipe;

namespace Home.Application.Tests.UseCases.Recipes.GetRecipe;

/// <summary>
/// One recipe, whole. The densest projection contract in the application: the presenter
/// dereferences the ingredient behind each join row, the meal slot behind each, and the note behind
/// each, and reads the steps besides. Four navigations, three of them dereferenced, and any one of
/// them left out of the query takes the recipe page down.
/// <para>
/// This used to be checked against a mocked context, which could see the filter and the not-found
/// path but nothing about what the query loads — a mock hands over a graph that is already
/// connected. See the 3 Sep decision.
/// </para>
/// </summary>
public class GetRecipeInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetRecipePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Recipe BuildRecipe(long recipeID, Household household, string name = "Spaghetti Bolognese")
        => new()
        {
            CookMinutes = 30,
            Household = household,
            Name = name,
            PrepMinutes = 10,
            RecipeID = recipeID,
            Servings = 4,
            Url = $"https://example.test/{recipeID}"
        };

    /// <summary>
    /// The full graph, with everything seeded out of order so the presenter has to sort it.
    /// </summary>
    private Recipe BuildWholeRecipe()
    {
        var _Recipe = this.BuildRecipe(120, this.Ours);

        _Recipe.Ingredients =
        [
            new RecipeIngredient()
            {
                Ingredient = new Ingredient() { Amount = 2, IngredientID = 131, Name = "Onion", Unit = MeasurementUnitSE.Pieces.Value },
                Recipe = _Recipe,
                Sequence = 2
            },
            new RecipeIngredient()
            {
                Ingredient = new Ingredient() { Amount = 500, IngredientID = 130, Name = "Beef mince", Unit = MeasurementUnitSE.Grams.Value },
                Recipe = _Recipe,
                Sequence = 1
            }
        ];

        _Recipe.MealSlots =
        [
            new RecipeMealSlot() { MealSlot = new MealSlot() { Household = this.Ours, MealSlotID = 111, Name = "Dinner", Sequence = 3 }, Recipe = _Recipe },
            new RecipeMealSlot() { MealSlot = new MealSlot() { Household = this.Ours, MealSlotID = 110, Name = "Lunch", Sequence = 2 }, Recipe = _Recipe }
        ];

        _Recipe.Notes =
        [
            new RecipeNote()
            {
                Note = new Note() { Content = "Freezes well", CreatedOnUTC = new DateTime(2026, 8, 12), NoteID = 140 },
                Recipe = _Recipe
            }
        ];

        _Recipe.Steps =
        [
            new RecipeStep() { Content = "Simmer for twenty minutes", RecipeStepID = 151, Sequence = 2, Title = "Cook" },
            new RecipeStep() { Content = "Chop the onion", RecipeStepID = 150, Sequence = 1, Title = "Prep" }
        ];

        return _Recipe;
    }

    private Task HandleAsync(long recipeID, CancellationToken cancellationToken = default)
        => new GetRecipeInteractor().HandleAsync(
            new GetRecipeInputPort(recipeID),
            this.m_Presenter,
            this.Services().Build(),
            cancellationToken);

    [Fact]
    public async Task HandleAsync_ReadsTheIngredientBehindEveryJoinRowInTheRecipesOwnOrder()
    {
        _ = this.Database.Seed(this.BuildWholeRecipe());

        await this.HandleAsync(120);

        var _Ingredients = Ok<GetRecipeApiResponse>(this.m_Presenter).Ingredients;

        _ = _Ingredients.Select(i => i.Name).Should().Equal(
            ["Beef mince", "Onion"],
            "the order lives on the join, because the position belongs to the recipe and not the thing itself");
        _ = _Ingredients.Select(i => i.Amount).Should().Equal([500, 2]);
    }

    [Fact]
    public async Task HandleAsync_ReadsTheMealSlotAndTheNoteBehindTheirJoinRowsToo()
    {
        _ = this.Database.Seed(this.BuildWholeRecipe());

        await this.HandleAsync(120);

        var _Response = Ok<GetRecipeApiResponse>(this.m_Presenter);

        _ = _Response.MealSlots.Select(ms => ms.Name).Should().Equal(["Lunch", "Dinner"]);
        _ = _Response.Notes.Select(n => n.Content).Should().Equal(["Freezes well"]);
    }

    [Fact]
    public async Task HandleAsync_ReturnsTheStepsInCookingOrder()
    {
        _ = this.Database.Seed(this.BuildWholeRecipe());

        await this.HandleAsync(120);

        _ = Ok<GetRecipeApiResponse>(this.m_Presenter).Steps
            .Select(s => s.Title).Should().Equal(["Prep", "Cook"]);
    }

    [Fact]
    public async Task HandleAsync_BringsBackTheRecipesOwnDetails()
    {
        _ = this.Database.Seed(this.BuildWholeRecipe());

        await this.HandleAsync(120);

        var _Response = Ok<GetRecipeApiResponse>(this.m_Presenter);

        _ = _Response.RecipeID.Should().Be(120);
        _ = _Response.Name.Should().Be("Spaghetti Bolognese");
        _ = _Response.CookMinutes.Should().Be(30);
        _ = _Response.PrepMinutes.Should().Be(10);
        _ = _Response.Servings.Should().Be(4);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(
            this.BuildRecipe(120, this.Ours),
            this.BuildRecipe(920, this.Theirs, "Their secret"));

        await this.HandleAsync(920);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchRecipeExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildRecipe(120, this.Ours));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenTheBookIsEmpty_PresentsNotFound()
    {
        await this.HandleAsync(120);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
