using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.GetRecipes;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Recipes.GetRecipes;
using Home.WebApi.UseCases.Recipes.GetRecipes;

namespace Home.Application.Tests.UseCases.Recipes.GetRecipes;

/// <summary>
/// What the planner remembers about a recipe, read off the meal plan at list time. The clock is
/// fixed at 12 August 2026, so "up to today" has a today.
/// </summary>
public class RecipeMealHistoryTests : InteractorTest
{

    #region Fields

    private readonly GetRecipesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealPlanEntry Planned(long id, Recipe recipe, DateTime date)
        => new() { Date = date, Household = recipe.Household, MealPlanEntryID = id, Recipe = recipe };

    [Fact]
    public async Task HandleAsync_SaysWhenARecipeWasLastHadAndWhenItIsNext()
    {
        var _Bolognese = new Recipe() { Household = this.Ours, Name = "Bolognese", RecipeID = 120, Url = "https://example.test/1" };
        var _Tacos = new Recipe() { Household = this.Ours, Name = "Tacos", RecipeID = 121, Url = "https://example.test/2" };

        _ = this.Database.Seed(
            Planned(150, _Bolognese, new DateTime(2026, 7, 1)),
            Planned(151, _Bolognese, new DateTime(2026, 8, 9)),
            Planned(152, _Bolognese, new DateTime(2026, 8, 12)),
            Planned(153, _Bolognese, new DateTime(2026, 8, 20)),
            _Tacos);

        await new GetRecipesInteractor().HandleAsync(new GetRecipesInputPort(null), this.m_Presenter, this.Services().Build(), CancellationToken.None);

        var _Recipes = Ok<GetRecipesApiResponse>(this.m_Presenter).Recipes.ToDictionary(r => r.Name);

        _ = _Recipes["Bolognese"].LastHadDate.Should().Be(new DateOnly(2026, 8, 12), "today counts as had");
        _ = _Recipes["Bolognese"].NextPlannedDate.Should().Be(new DateOnly(2026, 8, 20));
        _ = _Recipes["Bolognese"].TimesHad.Should().Be(3, "the future one is planned, not had");
        _ = _Recipes["Tacos"].LastHadDate.Should().BeNull();
        _ = _Recipes["Tacos"].TimesHad.Should().Be(0);
    }

    #endregion Methods

}
