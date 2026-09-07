using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Components.Pages.ShoppingList.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingListPage
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();

    /// <summary>
    /// Held by the page rather than either pane, because a drag starts in the items and can finish
    /// on a list, and neither of those two components can see the other.
    /// </summary>
    private readonly ShoppingListDrag m_Drag = new();

    #endregion Fields

    #region Properties

    [Parameter] public long? ShoppingListID { get; set; }

    #endregion Properties

}
