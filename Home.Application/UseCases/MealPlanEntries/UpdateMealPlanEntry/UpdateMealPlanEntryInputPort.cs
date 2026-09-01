using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;

public record UpdateMealPlanEntryInputPort(
    PropertyChangeTracker<DateTime> Date,
    long MealPlanEntryID,
    PropertyChangeTracker<long?> MealSlotID)
    : IInputPort<IUpdateMealPlanEntryOutputPort>;