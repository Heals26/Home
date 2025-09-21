using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;

public class CreateShoppingCartItemInputPort : IInputPort<ICreateShoppingCartItemOutputPort>
{

    #region Properties

    public int Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public long ShoppingCartID { get; init; }

    #endregion Properties

}
