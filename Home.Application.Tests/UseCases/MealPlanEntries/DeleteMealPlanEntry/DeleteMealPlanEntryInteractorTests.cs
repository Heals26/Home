using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.MealPlanEntries.DeleteMealPlanEntry;
using Home.Domain.Entities;
using Home.WebApi.Presenters.MealPlanEntries.DeleteMealPlanEntry;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.MealPlanEntries.DeleteMealPlanEntry;

/// <summary>
/// Taking a meal off the plan. The recipe stays in the book: only the plan entry goes.
/// </summary>
public class DeleteMealPlanEntryInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteMealPlanEntryPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealPlanEntry BuildEntry(long mealPlanEntryID, Recipe recipe)
        => new()
        {
            Date = new DateTime(2026, 8, 12),
            MealPlanEntryID = mealPlanEntryID,
            Recipe = recipe
        };

    private static Recipe BuildRecipe(long recipeID, Household household)
        => new()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

    private Task HandleAsync(long mealPlanEntryID)
        => new DeleteMealPlanEntryInteractor().HandleAsync(
            new DeleteMealPlanEntryInputPort(mealPlanEntryID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_TakesTheMealOffThePlanAndLeavesTheRecipeInTheBook()
    {
        _ = this.Database.Seed(BuildEntry(150, BuildRecipe(120, this.Ours)));

        await this.HandleAsync(150);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<MealPlanEntry>().Should().BeEmpty();
        _ = this.Stored<Recipe>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheEntryBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            BuildEntry(150, BuildRecipe(120, this.Ours)),
            BuildEntry(950, BuildRecipe(920, this.Theirs)));

        await this.HandleAsync(950);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<MealPlanEntry>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchEntryExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildEntry(150, BuildRecipe(120, this.Ours)));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
