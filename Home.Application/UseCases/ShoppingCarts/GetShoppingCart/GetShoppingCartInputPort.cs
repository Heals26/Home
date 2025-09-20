using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.GetShoppingCart;

public class GetShoppingCartInputPort : IInputPort<IGetShoppingCartOutputPort>
{

    #region Properties

    public long ShoppingCartID { get; set; }

    #endregion Properties

}
