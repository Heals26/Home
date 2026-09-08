namespace Home.Application.UseCases.Recipes.Models;

/// <summary>
/// What the planner remembers about a recipe: the last day it was on the plan up to today, the
/// next day it is planned for, and how many times it has been had. Read off the meal plan, never
/// stored, so it can never drift from it.
/// </summary>
public record RecipeMealHistory(DateOnly? LastHadDate, DateOnly? NextPlannedDate, int TimesHad)
{

    #region Fields

    public static readonly RecipeMealHistory None = new(null, null, 0);

    #endregion Fields

}
