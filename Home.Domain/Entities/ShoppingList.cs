namespace Home.Domain.Entities;

public class ShoppingList
{

    #region Properties

    public long ShoppingListID { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Audit> Audits { get; set; } = [];

    /// <summary>
    /// Kept on the list rather than the device, because a list is shopped from more than one phone
    /// and the grouping should read the same on all of them.
    /// </summary>
    public bool GroupByAisle { get; set; }

    /// <summary>
    /// A finished list the household has put away. Archiving keeps everything (the items, what
    /// they cost, what was ticked) and only takes the list out of the picker, so last Christmas's
    /// shop can still be duplicated a year later. Deleting is the destructive one.
    /// </summary>
    public bool IsArchived { get; set; }

    public Household Household { get; set; } = null!;
    public ICollection<ShoppingListItem> Items { get; set; } = [];
    public ICollection<ShoppingTrip> Trips { get; set; } = [];

    #endregion Properties

}
