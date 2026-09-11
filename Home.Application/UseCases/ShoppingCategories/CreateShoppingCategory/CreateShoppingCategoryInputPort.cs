using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCategories.CreateShoppingCategory;

public record CreateShoppingCategoryInputPort(string Name) : IInputPort<ICreateShoppingCategoryOutputPort>;
