using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;
using Home.Domain.Entities;
using Home.WebApi.Presenters.MealPlanEntries.UpdateMealPlanEntry;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.MealPlanEntries.UpdateMealPlanEntry;

/// <summary>
/// Moving a planned meal to another day or another meal of the day. The household is reached
/// through the recipe, the same path the entry itself hangs from.
/// </summary>
public class UpdateMealPlanEntryInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateMealPlanEntryPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealPlanEntry BuildEntry(long mealPlanEntryID, Recipe recipe, DateTime date, MealSlot? mealSlot = null)
        => new()
        {
            Date = date,
            MealPlanEntryID = mealPlanEntryID,
            MealSlot = mealSlot,
            Recipe = recipe
        };

    private static MealSlot BuildSlot(long mealSlotID, Household household, string name)
        => new()
        {
            Household = household,
            MealSlotID = mealSlotID,
            Name = name,
            Sequence = 1
        };

    private static Recipe BuildRecipe(long recipeID, Household household)
        => new()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

    private Task HandleAsync(long mealPlanEntryID, PropertyChangeTracker<DateTime> date = default, PropertyChangeTracker<long?> mealSlotID = default)
        => new UpdateMealPlanEntryInteractor().HandleAsync(
            new UpdateMealPlanEntryInputPort(date, mealPlanEntryID, mealSlotID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_MovesTheMealToAnotherDay()
    {
        _ = this.Database.Seed(BuildEntry(150, BuildRecipe(120, this.Ours), new DateTime(2026, 8, 12)));

        await this.HandleAsync(150, date: new(new DateTime(2026, 8, 14)));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<MealPlanEntry>().Single().Date.Should().Be(new DateTime(2026, 8, 14));
    }

    [Fact]
    public async Task HandleAsync_KeepsOnlyTheDayFromWhateverTimeArrives()
    {
        _ = this.Database.Seed(BuildEntry(150, BuildRecipe(120, this.Ours), new DateTime(2026, 8, 12)));

        await this.HandleAsync(150, date: new(new DateTime(2026, 8, 14, 17, 45, 0)));

        _ = this.Stored<MealPlanEntry>().Single().Date.Should().Be(
            new DateTime(2026, 8, 14),
            "entries always store midnight, because only the day is meaningful");
    }

    [Fact]
    public async Task HandleAsync_MovesTheMealToAnotherSlot()
    {
        var _Recipe = BuildRecipe(120, this.Ours);
        var _Breakfast = BuildSlot(110, this.Ours, "Breakfast");

        _ = this.Database.Seed(BuildSlot(111, this.Ours, "Dinner"), BuildEntry(150, _Recipe, new DateTime(2026, 8, 12), _Breakfast));

        await this.HandleAsync(150, mealSlotID: new(111));

        _ = this.Stored<MealPlanEntry>().Count(e => e.MealSlot != null && e.MealSlot.MealSlotID == 111).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_CanTakeTheMealOutOfEverySlot()
    {
        var _Breakfast = BuildSlot(110, this.Ours, "Breakfast");

        _ = this.Database.Seed(BuildEntry(150, BuildRecipe(120, this.Ours), new DateTime(2026, 8, 12), _Breakfast));

        await this.HandleAsync(150, mealSlotID: new(null));

        _ = this.Stored<MealPlanEntry>().Count(e => e.MealSlot == null).Should().Be(
            1,
            "clearing a navigation only works if it was loaded, which is why the query projects it");
    }

    [Fact]
    public async Task HandleAsync_WhenTheSlotBelongsToAnotherHousehold_ClearsTheSlotRatherThanMovingTheMealOntoTheirs()
    {
        var _Breakfast = BuildSlot(110, this.Ours, "Breakfast");

        _ = this.Database.Seed(
            BuildSlot(910, this.Theirs, "Theirs"),
            BuildEntry(150, BuildRecipe(120, this.Ours), new DateTime(2026, 8, 12), _Breakfast));

        await this.HandleAsync(150, mealSlotID: new(910));

        _ = this.Stored<MealPlanEntry>().Count(e => e.MealSlot == null).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheDateIsSent_LeavesTheSlotAlone()
    {
        var _Breakfast = BuildSlot(110, this.Ours, "Breakfast");

        _ = this.Database.Seed(BuildEntry(150, BuildRecipe(120, this.Ours), new DateTime(2026, 8, 12), _Breakfast));

        await this.HandleAsync(150, date: new(new DateTime(2026, 8, 14)));

        _ = this.Stored<MealPlanEntry>().Count(e => e.MealSlot != null && e.MealSlot.MealSlotID == 110).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheEntryBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(BuildEntry(950, BuildRecipe(920, this.Theirs), new DateTime(2026, 8, 12)));

        await this.HandleAsync(950, date: new(new DateTime(2026, 8, 14)));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<MealPlanEntry>().Single().Date.Should().Be(new DateTime(2026, 8, 12));
    }

    #endregion Methods

}
