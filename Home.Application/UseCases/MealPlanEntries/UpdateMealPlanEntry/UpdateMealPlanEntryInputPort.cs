using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;

/// <summary>
/// <paramref name="Title"/> is the name of an occasion, the planned meal that is not a recipe. A
/// meal that is a recipe takes its name from the recipe and ignores this.
/// </summary>
public record UpdateMealPlanEntryInputPort(
    PropertyChangeTracker<DateTime> Date,
    long MealPlanEntryID,
    PropertyChangeTracker<long?> MealSlotID,
    PropertyChangeTracker<string> Title)
    : IInputPort<IUpdateMealPlanEntryOutputPort>;
