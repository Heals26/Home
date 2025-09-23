using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ShoppingCarts.UpdateShoppingCart;

public class UpdateShoppingCartWebAppRequest
{

    #region Properties

    /// <summary>
    /// The name of the shopping cart
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    #endregion Properties

}
