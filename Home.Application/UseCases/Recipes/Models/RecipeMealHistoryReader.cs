using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.Models;

/// <summary>
/// One query over the household's meal plan, folded per recipe. Shared by the recipe list and the
/// recipe page so "last had" means the same thing on both.
/// </summary>
public static class RecipeMealHistoryReader
{

    #region Methods

    public static IReadOnlyDictionary<long, RecipeMealHistory> Read(
        IPersistenceContext persistenceContext,
        long householdID,
        DateOnly today,
        long? recipeID = null)
    {
        var _Today = today.ToDateTime(TimeOnly.MinValue);

        var _Entries = persistenceContext.GetEntities<MealPlanEntry>()
            .Where(e => e.Household.HouseholdID == householdID
                && e.Recipe != null
                && (recipeID == null || e.Recipe.RecipeID == recipeID))
            .Select(e => new { RecipeID = e.Recipe!.RecipeID, e.Date })
            .ToList();

        return _Entries
            .GroupBy(e => e.RecipeID)
            .ToDictionary(
                g => g.Key,
                g => new RecipeMealHistory(
                    g.Where(e => e.Date <= _Today).Select(e => (DateOnly?)DateOnly.FromDateTime(e.Date)).Max(),
                    g.Where(e => e.Date > _Today).Select(e => (DateOnly?)DateOnly.FromDateTime(e.Date)).Min(),
                    g.Count(e => e.Date <= _Today)));
    }

    #endregion Methods

}
