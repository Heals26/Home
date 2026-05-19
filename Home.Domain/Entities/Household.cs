namespace Home.Domain.Entities;

public class Household
{

    #region Properties

    public long HouseholdID { get; set; }
    public string Name { get; set; }

    public ICollection<Recipe> Recipes { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
    public ICollection<User> Members { get; set; }

    #endregion Properties

}
