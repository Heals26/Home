namespace Home.WebApi.UseCases.ShoppingLists.StartShoppingTrip;

public class StartShoppingTripApiResponse
{

    #region Properties

    /// <summary>
    /// The trip started or joined, which the device remembers to know it is shopping.
    /// </summary>
    public long ShoppingTripID { get; set; }

    #endregion Properties

}
