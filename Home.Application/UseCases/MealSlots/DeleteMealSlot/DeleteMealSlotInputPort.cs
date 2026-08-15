using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.MealSlots.DeleteMealSlot;

public record DeleteMealSlotInputPort(long MealSlotID) : IInputPort<IDeleteMealSlotOutputPort>;
