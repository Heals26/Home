using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.MealSlots.UpdateMealSlot;

public record UpdateMealSlotInputPort(
    long MealSlotID,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence,
    PropertyChangeTracker<TimeSpan?> StartsAt)
    : IInputPort<IUpdateMealSlotOutputPort>;
