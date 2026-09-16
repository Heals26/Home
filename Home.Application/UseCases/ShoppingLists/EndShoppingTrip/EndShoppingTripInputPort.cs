using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.EndShoppingTrip;

public record EndShoppingTripInputPort(long ShoppingListID)
    : IInputPort<IEndShoppingTripOutputPort>;
