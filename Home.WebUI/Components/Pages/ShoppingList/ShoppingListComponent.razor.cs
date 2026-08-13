using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.ShoppingListItems.CreateShoppingListItem;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingList;
using Home.WebUI.Infrastructure.ApiProviders;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingListComponent
{

    #region Fields

    private ErrorHandler? m_ErrorHandler;
    private GetShoppingListWebAppResponse? m_ShoppingList;
    private CreateShoppingListItemWebAppRequest? m_AddItemRequest = new();
    private bool m_ShowAddItem;
    private bool m_AddingItem;
    private string m_QuantityInput = string.Empty;
    private string m_CostInput = string.Empty;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [Parameter] public long? ShoppingListID { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnParametersSetAsync()
    {
        if (this.ShoppingListID.HasValue)
            await this.LoadListAsync();
        else
            this.m_ShoppingList = null;
    }

    #endregion Lifecycle Methods

    #region Methods

    private void OpenAddItemModal()
    {
        this.m_AddItemRequest = new() { ShoppingListID = this.ShoppingListID!.Value };
        this.m_QuantityInput = string.Empty;
        this.m_CostInput = string.Empty;
        this.m_ShowAddItem = true;
    }

    private async Task LoadListAsync()
    {
        this.m_ShoppingList = await this.ApiAccess.SendRequestAsync<object, GetShoppingListWebAppResponse>(
            null!, ApiProvider.GetShoppingList(this.ShoppingListID!.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);
    }

    private async Task AddItemAsync()
    {
        if (this.m_AddingItem) return;
        this.m_AddingItem = true;

        this.m_AddItemRequest!.Quantity = decimal.TryParse(this.m_QuantityInput, out var _Qty) ? _Qty : null;
        this.m_AddItemRequest!.Cost = decimal.TryParse(this.m_CostInput, out var _Cost) ? _Cost : null;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateShoppingListItemWebAppRequest, CreateShoppingListItemWebAppResponse>(
            this.m_AddItemRequest!,
            ApiProvider.CreateShoppingListItem(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_AddingItem = false;

        if (_Result == null)
            return;

        this.m_ShowAddItem = false;
        this.m_AddItemRequest = new() { ShoppingListID = this.ShoppingListID!.Value };

        await this.LoadListAsync();
    }

    #endregion Methods

}
