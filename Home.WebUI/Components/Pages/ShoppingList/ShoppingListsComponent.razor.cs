using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.ShoppingLists.CreateShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingListsComponent : IDisposable
{

    #region Fields

    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;
    private CreateShoppingListWebAppRequest? m_CreateRequest = new();
    private GetShoppingListsWebAppResponse? m_ShoppingLists;
    private bool m_ShowCreate;
    private bool m_Creating;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [Parameter] public long? ShoppingListID { get; set; }
    [Parameter] public EventCallback<long> ShoppingListIDChanged { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await this.LoadListsAsync();

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.CancellationToken);
    }

    public void Dispose()
        => this.m_ChangeSubscription?.Dispose();

    #endregion Lifecycle Methods

    #region Methods

    private async Task LoadListsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetShoppingListsWebAppResponse>(
            null!, ApiProvider.GetShoppingLists(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != null)
            this.m_ShoppingLists = _Result;
    }

    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        if (area != ChangeArea.ShoppingLists)
            return;

        await this.InvokeAsync(async () =>
        {
            await this.LoadListsAsync();
            this.StateHasChanged();
        });
    }

    private void OpenCreateModal()
        => this.m_ShowCreate = true;

    private void SelectList(GetShoppingListDto list)
        => this.NavigationManager.NavigateTo($"/shopping-lists/{list.ShoppingListID}");

    private async Task CreateShoppingListAsync()
    {
        if (this.m_Creating) return;
        this.m_Creating = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateShoppingListWebAppRequest, CreateShoppingListWebAppResponse>(
            this.m_CreateRequest!,
            ApiProvider.CreateShoppingList(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Creating = false;

        if (_Result == null)
            return;

        this.m_ShowCreate = false;
        this.m_CreateRequest = new();

        await this.LoadListsAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    private async Task DeleteShoppingListAsync(long shoppingListID)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteShoppingList(shoppingListID),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != true)
            return;

        if (this.ShoppingListID == shoppingListID)
            this.NavigationManager.NavigateTo("/shopping-lists");

        await this.LoadListsAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    #endregion Methods

}
