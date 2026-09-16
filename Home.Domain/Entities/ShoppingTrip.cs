namespace Home.Domain.Entities;

/// <summary>
/// One shop with a list, from when someone switches shopping mode on until Done. Only a line ticked
/// during one has its price remembered.
/// </summary>
public class ShoppingTrip
{

    #region Properties

    public long ShoppingTripID { get; set; }

    /// <summary>
    /// When the trip was ended. A trip nobody has touched for the quiet spell is over without this
    /// ever being written.
    /// </summary>
    public DateTime? EndedOnUTC { get; set; }

    /// <summary>
    /// When someone last joined the trip or changed a line on the list during it, which the quiet
    /// spell is measured from.
    /// </summary>
    public DateTime LastActivityOnUTC { get; set; }

    public DateTime StartedOnUTC { get; set; }

    public ShoppingList ShoppingList { get; set; } = null!;

    #endregion Properties

}
