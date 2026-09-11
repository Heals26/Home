using Home.WebUI.DataAccess.ShoppingCategories.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingAisleRow
{

    #region Fields

    private bool m_ConfirmingRemove;

    #endregion Fields

    #region Properties

    [Parameter] public ShoppingCategoryDto Aisle { get; set; } = null!;
    [Parameter] public bool Busy { get; set; }
    [Parameter] public bool CanMoveDown { get; set; }
    [Parameter] public bool CanMoveUp { get; set; }
    [Parameter] public string? Error { get; set; }
    [Parameter] public EventCallback OnMoveDown { get; set; }
    [Parameter] public EventCallback OnMoveUp { get; set; }
    [Parameter] public EventCallback OnRemove { get; set; }

    /// <summary>
    /// Raised when the name box is left, with whatever it holds.
    /// </summary>
    [Parameter] public EventCallback<string> OnRename { get; set; }

    #endregion Properties

}
