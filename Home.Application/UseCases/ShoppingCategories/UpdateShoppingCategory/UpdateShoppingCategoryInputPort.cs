using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingCategories.UpdateShoppingCategory;

public record UpdateShoppingCategoryInputPort(
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence,
    long ShoppingCategoryID)
    : IInputPort<IUpdateShoppingCategoryOutputPort>;
