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
    private long? m_LoadedShoppingListID;
    private bool m_LoadingList;
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
        if (this.ShoppingListID == this.m_LoadedShoppingListID)
            return;

        // Everything on screen belongs to the list we are leaving, including a half-filled
        // add-item modal, so none of it may survive the switch.
        this.m_LoadedShoppingListID = this.ShoppingListID;
        this.m_ShoppingList = null;
        this.m_ShowAddItem = false;
        this.m_AddItemRequest = new();
        this.m_QuantityInput = string.Empty;
        this.m_CostInput = string.Empty;

        if (this.ShoppingListID.HasValue)
            await this.LoadListAsync();
        else
            this.m_LoadingList = false;
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
        this.m_AddItemRequest = new();
        this.m_QuantityInput = string.Empty;
        this.m_CostInput = string.Empty;
        this.m_ShowAddItem = true;
    }

    private async Task LoadListAsync()
    {
        var _RequestedShoppingListID = this.ShoppingListID!.Value;

        this.m_LoadingList = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetShoppingListWebAppResponse>(
            null!, ApiProvider.GetShoppingList(_RequestedShoppingListID),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        // A response for a list the user has already left would otherwise land under the new
        // list's heading, so it is dropped.
        if (_RequestedShoppingListID != this.ShoppingListID)
            return;

        this.m_LoadingList = false;
        this.m_ShoppingList = _Result;
    }

    private async Task AddItemAsync()
    {
        if (this.m_AddingItem || !this.ShoppingListID.HasValue) return;
        this.m_AddingItem = true;

        // The target list is read at submit time, never at modal-open time.
        var _Request = new CreateShoppingListItemWebAppRequest()
        {
            Cost = decimal.TryParse(this.m_CostInput, out var _Cost) ? _Cost : null,
            Name = this.m_AddItemRequest!.Name,
            Quantity = decimal.TryParse(this.m_QuantityInput, out var _Quantity) ? _Quantity : null,
            ShoppingListID = this.ShoppingListID.Value
        };

        var _Result = await this.ApiAccess.SendRequestAsync<CreateShoppingListItemWebAppRequest, CreateShoppingListItemWebAppResponse>(
            _Request,
            ApiProvider.CreateShoppingListItem(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_AddingItem = false;

        if (_Result == null)
            return;

        this.m_ShowAddItem = false;
        this.m_AddItemRequest = new();
        this.m_QuantityInput = string.Empty;
        this.m_CostInput = string.Empty;

        if (_Request.ShoppingListID == this.ShoppingListID)
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
