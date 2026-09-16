using Home.WebUI.Infrastructure.Services.ShoppingLists;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingTrolleyBar
{

    #region Properties

    /// <summary>
    /// What the trolley comes to, with an unpriced line taken at what it cost last time.
    /// </summary>
    [Parameter, EditorRequired] public ShoppingListEstimate Estimate { get; set; } = null!;

    [Parameter] public bool Finishing { get; set; }
    [Parameter] public int InTrolley { get; set; }
    [Parameter] public int ItemCount { get; set; }
    [Parameter] public EventCallback OnDone { get; set; }
    [Parameter] public int ProgressPercent { get; set; }

    #endregion Properties

}
