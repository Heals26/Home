namespace Home.WebUI.DataAccess.ShoppingListItems.SetShoppingListItemSequence;

/// <summary>
/// Moves a line to a position in its list.
/// </summary>
public class SetShoppingListItemSequenceWebAppRequest
{

    #region Properties

    /// <summary>
    /// Where the line lands. The lines it passes move up or down to make room.
    /// </summary>
    public long Sequence { get; set; }

    #endregion Properties

}
