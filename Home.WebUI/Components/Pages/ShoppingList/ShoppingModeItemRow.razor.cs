using Home.WebUI.DataAccess.ShoppingLists.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingModeItemRow
{

    #region Properties

    [Parameter, EditorRequired] public ShoppingListItemDto Item { get; set; } = null!;
    [Parameter] public EventCallback<ShoppingListItemDto> OnEdit { get; set; }
    [Parameter] public EventCallback<ShoppingListItemDto> OnToggle { get; set; }

    #endregion Properties

}
