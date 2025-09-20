using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingCarts.UpdateShoppingCart;

public class UpdateShoppingCartInputPort : IInputPort<IUpdateShoppingCartOutputPort>
{

    #region Properties

    public PropertyChangeTracker<string> Name { get; set; }
    public long ShoppingCartID { get; set; }

    #endregion Properties

}
