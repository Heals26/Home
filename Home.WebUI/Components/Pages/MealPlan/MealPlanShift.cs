using Home.WebUI.DataAccess.MealPlanEntries.Models;

namespace Home.WebUI.Components.Pages.MealPlan;

/// <summary>
/// A planned meal being nudged a whole number of days earlier or later, keeping the meal of the
/// day it was already on.
/// </summary>
/// <param name="Entry">The planned meal being moved.</param>
/// <param name="Days">How many days to shift it — negative is earlier.</param>
public record MealPlanShift(MealPlanEntryDto Entry, int Days);
