namespace Home.WebApi.UseCases.MealSlots.CreateMealSlot;

public record CreateMealSlotApiRequest(string Name, TimeSpan? StartsAt);
