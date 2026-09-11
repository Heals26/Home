using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ShoppingCategories.UpdateShoppingCategory;

public class UpdateShoppingCategoryWebAppRequest
{

    #region Properties

    /// <summary>
    /// What the household calls this part of the shop.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// Where it comes on the household's walk through the shop.
    /// </summary>
    public PropertyChangeTracker<int> Sequence { get; set; }

    #endregion Properties

}
