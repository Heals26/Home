using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.MealSlots.CreateMealSlot;

public record CreateMealSlotInputPort(string Name, TimeSpan? StartsAt) : IInputPort<ICreateMealSlotOutputPort>;
