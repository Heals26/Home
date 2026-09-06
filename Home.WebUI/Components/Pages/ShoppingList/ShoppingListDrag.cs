using Home.WebUI.DataAccess.ShoppingLists.Models;

namespace Home.WebUI.Components.Pages.ShoppingList;

/// <summary>
/// What is currently being dragged around the shopping page, cascaded from the page so the two
/// panes can see the same drag. The item lives in one component and the lists it can be dropped on
/// live in another, and a drag that crosses between them has nowhere else to keep track of itself.
/// <para>
/// It raises <see cref="Changed"/> because a cascaded object that is mutated rather than replaced
/// tells Blazor nothing: without this the lists pane never redraws and no list ever lights up as
/// somewhere the item could land.
/// </para>
/// <para>
/// Only a pointer device ever fills this in. Touch fires no drag events at all, which is why every
/// move a drag can make is also reachable from a button.
/// </para>
/// </summary>
public class ShoppingListDrag
{

    #region Events

    public event Action? Changed;

    #endregion Events

    #region Properties

    /// <summary>
    /// The list it came from, so a drop back onto its own list can be ignored.
    /// </summary>
    public long? FromShoppingListID { get; private set; }

    /// <summary>
    /// The item being dragged, or null when nothing is.
    /// </summary>
    public ShoppingListItemDto? Item { get; private set; }

    /// <summary>
    /// The list currently under the cursor, when the cursor has left the items and is over the
    /// picker. The items pane reads this to put its own drop line away: two indicators promising
    /// two different landing places at once is worse than neither.
    /// </summary>
    public long? OverShoppingListID { get; private set; }

    #endregion Properties

    #region Methods

    public void Clear()
    {
        if (this.Item == null && this.OverShoppingListID == null)
            return;

        this.Item = null;
        this.FromShoppingListID = null;
        this.OverShoppingListID = null;

        this.Changed?.Invoke();
    }

    /// <summary>
    /// The cursor moved over one of the lists in the picker.
    /// </summary>
    public void HoverList(long shoppingListID)
    {
        if (this.OverShoppingListID == shoppingListID)
            return;

        this.OverShoppingListID = shoppingListID;

        this.Changed?.Invoke();
    }

    public void Start(ShoppingListItemDto item, long? fromShoppingListID)
    {
        this.Item = item;
        this.FromShoppingListID = fromShoppingListID;
        this.OverShoppingListID = null;

        this.Changed?.Invoke();
    }

    /// <summary>
    /// The cursor came back off the picker and onto the items.
    /// </summary>
    public void LeaveLists()
    {
        if (this.OverShoppingListID == null)
            return;

        this.OverShoppingListID = null;

        this.Changed?.Invoke();
    }

    #endregion Methods

}
