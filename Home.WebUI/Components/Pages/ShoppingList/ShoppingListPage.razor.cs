using Home.WebUI.Infrastructure.CancellationTokens;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingListPage
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();

    #endregion Fields

    #region Properties

    [Parameter] public long? ShoppingListID { get; set; }

    #endregion Properties

}
