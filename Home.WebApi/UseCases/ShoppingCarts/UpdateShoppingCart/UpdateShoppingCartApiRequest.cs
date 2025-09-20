using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ShoppingCarts.UpdateShoppingCart;

public class UpdateShoppingCartApiRequest
{

    #region Properties

    /// <summary>
    /// The name of the shopping cart
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    #endregion Properties

}
