using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingModeStart
{

    #region Properties

    [Parameter] public bool Busy { get; set; }
    [Parameter] public EventCallback OnStart { get; set; }

    /// <summary>
    /// Someone is already shopping with the list, so this joins their shop rather than starting one.
    /// </summary>
    [Parameter] public bool ShopInProgress { get; set; }

    #endregion Properties

}
