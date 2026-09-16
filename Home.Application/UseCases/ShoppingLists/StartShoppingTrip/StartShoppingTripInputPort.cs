using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.StartShoppingTrip;

public record StartShoppingTripInputPort(long ShoppingListID)
    : IInputPort<IStartShoppingTripOutputPort>;
