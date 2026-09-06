using Home.WebUI.DataAccess.ShoppingLists.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingListItemRow
{

    #region Properties

    [Parameter] public bool CanMoveDown { get; set; }
    [Parameter] public bool CanMoveUp { get; set; }

    /// <summary>
    /// Whether this row can be picked up with a mouse. Off for anything already in the trolley,
    /// where the order is history and not worth arranging.
    /// </summary>
    [Parameter] public bool Draggable { get; set; }

    /// <summary>
    /// Where to draw the line showing a pending drop, which is the edge the item will land on
    /// rather than the one the cursor happens to be nearest.
    /// </summary>
    [Parameter] public ShoppingListDropLine DropLine { get; set; }

    /// <summary>
    /// Whether this is the row being carried, which is faded so it reads as picked up.
    /// </summary>
    [Parameter] public bool IsBeingDragged { get; set; }

    [Parameter, EditorRequired] public ShoppingListItemDto Item { get; set; } = null!;
    [Parameter] public EventCallback OnDragEnd { get; set; }
    [Parameter] public EventCallback<ShoppingListItemDto> OnDragEnter { get; set; }
    [Parameter] public EventCallback<ShoppingListItemDto> OnDragStart { get; set; }
    [Parameter] public EventCallback<ShoppingListItemDto> OnDrop { get; set; }
    [Parameter] public EventCallback<ShoppingListItemDto> OnEdit { get; set; }
    [Parameter] public EventCallback<ShoppingListItemDto> OnMoveDown { get; set; }
    [Parameter] public EventCallback<ShoppingListItemDto> OnMoveUp { get; set; }
    [Parameter] public EventCallback<ShoppingListItemDto> OnToggle { get; set; }
    [Parameter] public bool ShowReorder { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// The amount and the price on one line, so a row stays a row on a phone.
    /// </summary>
    private string Detail()
    {
        List<string> _Parts = [];

        var _Amount = ShoppingListItemLogic.DescribeAmount(this.Item);

        if (_Amount.Length > 0)
            _Parts.Add(_Amount);

        if (this.Item.Cost is > 0)
            _Parts.Add($"${this.Item.Cost.Value:F2}");

        return string.Join(" · ", _Parts);
    }

    #endregion Methods

}
