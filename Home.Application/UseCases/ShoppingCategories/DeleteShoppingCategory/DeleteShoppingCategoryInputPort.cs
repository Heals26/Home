using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCategories.DeleteShoppingCategory;

public record DeleteShoppingCategoryInputPort(long ShoppingCategoryID) : IInputPort<IDeleteShoppingCategoryOutputPort>;
