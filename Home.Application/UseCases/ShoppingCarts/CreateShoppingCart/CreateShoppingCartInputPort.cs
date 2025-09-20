using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.CreateShoppingCart;

public class CreateShoppingCartInputPort : IInputPort<ICreateShoppingCartOutputPort>
{

    #region Properties

    public string Name { get; set; }

    #endregion Properties

}
