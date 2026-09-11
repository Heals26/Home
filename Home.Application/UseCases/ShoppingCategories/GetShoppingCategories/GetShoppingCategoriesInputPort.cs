using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCategories.GetShoppingCategories;

public record GetShoppingCategoriesInputPort() : IInputPort<IGetShoppingCategoriesOutputPort>;
