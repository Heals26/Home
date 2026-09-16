using Home.WebUI.DataAccess.ShoppingLists.Models;
using Home.WebUI.Infrastructure.ShoppingLists;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingListItemText
{

    #region Properties

    [Parameter, EditorRequired] public ShoppingListItemDto Item { get; set; } = null!;

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
        else if (this.Item.EstimatedCost is > 0)
            _Parts.Add($"about ${this.Item.EstimatedCost.Value:F2}");

        return string.Join(" · ", _Parts);
    }

    #endregion Methods

}
