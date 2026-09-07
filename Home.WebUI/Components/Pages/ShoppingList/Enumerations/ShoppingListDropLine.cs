namespace Home.WebUI.Components.Pages.ShoppingList.Enumerations;

/// <summary>
/// Where the line showing a pending drop is drawn on a row, which is where the dragged item will
/// actually land rather than simply where the cursor is.
/// <para>
/// The side depends on which way the item is travelling. Dropping onto a row gives the item that
/// row's position: coming from above, everything in between shifts up and the item ends up below
/// the row it was dropped on; coming from below, it ends up above it.
/// </para>
/// </summary>
public enum ShoppingListDropLine
{
    None = 0,
    Above = 1,
    Below = 2,
}
