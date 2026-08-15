using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.MealSlots.GetMealSlots;

public record GetMealSlotsInputPort() : IInputPort<IGetMealSlotsOutputPort>;
