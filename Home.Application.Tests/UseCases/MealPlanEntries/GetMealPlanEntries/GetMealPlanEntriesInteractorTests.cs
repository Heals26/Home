using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.MealPlanEntries.GetMealPlanEntries;
using Home.Domain.Entities;
using Home.WebApi.Presenters.MealPlanEntries.GetMealPlanEntries;
using Home.WebApi.UseCases.MealPlanEntries.GetMealPlanEntries;

namespace Home.Application.Tests.UseCases.MealPlanEntries.GetMealPlanEntries;

/// <summary>
/// The week's meals. An entry carries no household of its own — it is reached through the recipe —
/// and the presenter names that recipe on every row, so the query has to load it.
/// </summary>
public class GetMealPlanEntriesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetMealPlanEntriesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealPlanEntry BuildEntry(long mealPlanEntryID, DateTime date, Recipe recipe, MealSlot? mealSlot = null)
        => new()
        {
            Date = date,
            MealPlanEntryID = mealPlanEntryID,
            MealSlot = mealSlot,
            Recipe = recipe
        };

    private static Recipe BuildRecipe(long recipeID, Household household, string name)
        => new()
        {
            Household = household,
            Name = name,
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

    private Task HandleAsync(DateTime fromDate, DateTime toDate)
        => new GetMealPlanEntriesInteractor().HandleAsync(
            new GetMealPlanEntriesInputPort(fromDate, toDate),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_NamesTheRecipeAndTheMealOnEveryEntry()
    {
        var _Dinner = new MealSlot() { Household = this.Ours, MealSlotID = 110, Name = "Dinner", Sequence = 3 };

        _ = this.Database.Seed(BuildEntry(150, new DateTime(2026, 8, 12), BuildRecipe(120, this.Ours, "Bolognese"), _Dinner));

        await this.HandleAsync(new DateTime(2026, 8, 10), new DateTime(2026, 8, 16));

        var _Entry = Ok<GetMealPlanEntriesApiResponse>(this.m_Presenter).Entries.Single();

        _ = _Entry.RecipeID.Should().Be(120);
        _ = _Entry.RecipeName.Should().Be(
            "Bolognese",
            "the presenter reads the recipe on every row, so the query has to load it");
        _ = _Entry.MealSlotID.Should().Be(110);
        _ = _Entry.MealSlotName.Should().Be("Dinner");
    }

    [Fact]
    public async Task HandleAsync_ReadsThroughTheDayAndPutsEntriesWithoutAMealLast()
    {
        var _Breakfast = new MealSlot() { Household = this.Ours, MealSlotID = 110, Name = "Breakfast", Sequence = 1 };
        var _Dinner = new MealSlot() { Household = this.Ours, MealSlotID = 111, Name = "Dinner", Sequence = 3 };
        var _Recipe = BuildRecipe(120, this.Ours, "Bolognese");

        _ = this.Database.Seed(
            BuildEntry(152, new DateTime(2026, 8, 12), _Recipe),
            BuildEntry(151, new DateTime(2026, 8, 12), _Recipe, _Dinner),
            BuildEntry(150, new DateTime(2026, 8, 12), _Recipe, _Breakfast),
            BuildEntry(153, new DateTime(2026, 8, 11), _Recipe, _Dinner));

        await this.HandleAsync(new DateTime(2026, 8, 10), new DateTime(2026, 8, 16));

        _ = Ok<GetMealPlanEntriesApiResponse>(this.m_Presenter).Entries
            .Select(e => e.MealPlanEntryID).Should().Equal(
                153, 150, 151, 152);
    }

    [Fact]
    public async Task HandleAsync_ReturnsOnlyTheDaysAskedFor()
    {
        var _Recipe = BuildRecipe(120, this.Ours, "Bolognese");

        _ = this.Database.Seed(
            BuildEntry(150, new DateTime(2026, 8, 9), _Recipe),
            BuildEntry(151, new DateTime(2026, 8, 10), _Recipe),
            BuildEntry(152, new DateTime(2026, 8, 16), _Recipe),
            BuildEntry(153, new DateTime(2026, 8, 17), _Recipe));

        await this.HandleAsync(new DateTime(2026, 8, 10), new DateTime(2026, 8, 16));

        _ = Ok<GetMealPlanEntriesApiResponse>(this.m_Presenter).Entries
            .Select(e => e.MealPlanEntryID).Should().Equal(
                [151, 152],
                "both ends of the range are inclusive, because a week runs to its last day");
    }

    [Fact]
    public async Task HandleAsync_NeverReturnsAnotherHouseholdsPlan()
    {
        _ = this.Database.Seed(
            BuildEntry(150, new DateTime(2026, 8, 12), BuildRecipe(120, this.Ours, "Bolognese")),
            BuildEntry(950, new DateTime(2026, 8, 12), BuildRecipe(920, this.Theirs, "Their dinner")));

        await this.HandleAsync(new DateTime(2026, 8, 10), new DateTime(2026, 8, 16));

        _ = Ok<GetMealPlanEntriesApiResponse>(this.m_Presenter).Entries
            .Select(e => e.RecipeName).Should().Equal(["Bolognese"]);
    }

    #endregion Methods

}
