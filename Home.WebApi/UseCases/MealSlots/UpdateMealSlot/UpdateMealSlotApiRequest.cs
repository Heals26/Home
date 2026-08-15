using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.MealSlots.UpdateMealSlot;

public record UpdateMealSlotApiRequest(
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence,
    PropertyChangeTracker<TimeSpan?> StartsAt);
