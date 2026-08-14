using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.ShoppingListItems.CreateShoppingListItem;
using Home.WebUI.DataAccess.ShoppingListItems.UpdateShoppingListItem;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingListComponent : IDisposable
{

    #region Fields

    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;
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

    protected override async Task OnInitializedAsync()
        => this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.CancellationToken);

    protected override async Task OnParametersSetAsync()
    {
        if (this.ShoppingListID.HasValue)
            await this.LoadListAsync();
        else
            this.m_ShoppingList = null;
    }

    public void Dispose()
        => this.m_ChangeSubscription?.Dispose();

    #endregion Lifecycle Methods

    #region Methods

    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        if (area != ChangeArea.ShoppingLists || !this.ShoppingListID.HasValue)
            return;

        await this.InvokeAsync(async () =>
        {
            await this.LoadListAsync();
            this.StateHasChanged();
        });
    }

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
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    // The tick flips immediately so the shop flow feels instant; a failed call reloads the truth.
    private async Task ToggleItemAsync(ShoppingListItemDto item)
    {
        item.InBasket = !item.InBasket;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateShoppingListItemWebAppRequest, bool>(
            new UpdateShoppingListItemWebAppRequest()
            {
                InBasket = new PropertyChangeTracker<bool>(item.InBasket),
                ShoppingListItemID = item.ShoppingListItemID
            },
            ApiProvider.UpdateShoppingListItem(item.ShoppingListItemID),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != true)
        {
            await this.LoadListAsync();
            return;
        }

        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    private decimal ListTotal()
        => (this.m_ShoppingList?.Items ?? []).Sum(i => (i.Cost ?? 0) * (i.Quantity ?? 1));

    private decimal TrolleyTotal()
        => (this.m_ShoppingList?.Items ?? []).Where(i => i.InBasket).Sum(i => (i.Cost ?? 0) * (i.Quantity ?? 1));

    #endregion Methods

}
