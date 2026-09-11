using Home.WebUI.DataAccess.ShoppingCategories.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingAisleHeading
{

    #region Properties

    /// <summary>
    /// Null over the items not filed under any aisle yet.
    /// </summary>
    [Parameter] public ShoppingCategoryDto? Aisle { get; set; }

    #endregion Properties

}
