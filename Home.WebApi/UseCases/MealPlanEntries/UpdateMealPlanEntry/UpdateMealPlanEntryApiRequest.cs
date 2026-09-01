using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.MealPlanEntries.UpdateMealPlanEntry;

public record UpdateMealPlanEntryApiRequest(
    PropertyChangeTracker<DateTime> Date,
    PropertyChangeTracker<long?> MealSlotID);