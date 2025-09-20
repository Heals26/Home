using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.DeleteShoppingCart;

public class DeleteShoppingCartInputPort : IInputPort<IDeleteShoppingCartOutputPort>
{

    #region Properties

    public long ShoppingCartID { get; set; }

    #endregion Properties

}
