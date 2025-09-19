using System;
using System.Collections.Generic;

namespace Home.Domain.Entities;

public class ShoppingList
{

    #region - - - - - - Properties - - - - - -

    public long ShoppingListID { get; set; }
    public string Name { get; set; }

    public ICollection<ShoppingListItem> Items { get; set; }

    #endregion Properties

}
