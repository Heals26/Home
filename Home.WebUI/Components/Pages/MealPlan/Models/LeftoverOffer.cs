namespace Home.WebUI.Components.Pages.MealPlan.Models;

/// <summary>
/// The one-tap offer made after a recipe is planned: the same meal as leftovers the next day.
/// Cook once, eat twice.
/// </summary>
public record LeftoverOffer(string RecipeName, DateTime Date, long? MealSlotID, string MealSlotName);
