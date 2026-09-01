using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.ShoppingLists.CreateShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.DuplicateShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.DataAccess.ShoppingLists.UpdateShoppingList;
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
    private bool m_ShowArchived;

    // Managing one list
    private bool m_ShowManage;
    private long? m_ManagingListID;
    private string m_ManageName = string.Empty;
    private bool m_ManageIsArchived;
    private bool m_Renaming;
    private bool m_Duplicating;
    private bool m_Archiving;
    private bool m_Deleting;

    /// <summary>
    /// Deleting a list takes everything on it and there is no undo, so the button asks twice
    /// rather than opening a second modal on top of this one.
    /// </summary>
    private bool m_ConfirmingDelete;

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
        if (this.m_Creating || string.IsNullOrWhiteSpace(this.m_CreateRequest!.Name)) return;
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

    /// <summary>
    /// Archived lists stay out of the way unless asked for — except the one being looked at, which
    /// would otherwise vanish from under the reader the moment they archived it.
    /// </summary>
    private IEnumerable<GetShoppingListDto> VisibleLists()
        => (this.m_ShoppingLists?.ShoppingLists ?? [])
            .Where(l => !l.IsArchived || this.m_ShowArchived || l.ShoppingListID == this.ShoppingListID);

    private int ArchivedCount()
        => (this.m_ShoppingLists?.ShoppingLists ?? []).Count(l => l.IsArchived);

    private void OpenManageModal(GetShoppingListDto list)
    {
        this.m_ManagingListID = list.ShoppingListID;
        this.m_ManageName = list.Name;
        this.m_ManageIsArchived = list.IsArchived;
        this.m_ConfirmingDelete = false;
        this.m_ShowManage = true;
    }

    private async Task RenameShoppingListAsync()
    {
        if (this.m_Renaming || this.m_ManagingListID == null || string.IsNullOrWhiteSpace(this.m_ManageName))
            return;

        this.m_Renaming = true;

        var _Result = await this.SaveListAsync(new() { Name = new(this.m_ManageName.Trim()) });

        this.m_Renaming = false;

        if (_Result)
            this.m_ShowManage = false;
    }

    private async Task ToggleArchivedAsync()
    {
        if (this.m_Archiving || this.m_ManagingListID == null)
            return;

        this.m_Archiving = true;

        var _Result = await this.SaveListAsync(new() { IsArchived = new(!this.m_ManageIsArchived) });

        this.m_Archiving = false;

        if (!_Result)
            return;

        this.m_ManageIsArchived = !this.m_ManageIsArchived;
        this.m_ShowManage = false;
    }

    private async Task<bool> SaveListAsync(UpdateShoppingListWebAppRequest request)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<UpdateShoppingListWebAppRequest, bool>(
            request, ApiProvider.UpdateShoppingList(this.m_ManagingListID!.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != true)
            return false;

        await this.LoadListsAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);

        return true;
    }

    /// <summary>
    /// The copy opens straight away, because duplicating a list is something you do in order to
    /// start using it.
    /// </summary>
    private async Task DuplicateShoppingListAsync()
    {
        if (this.m_Duplicating || this.m_ManagingListID == null)
            return;

        this.m_Duplicating = true;

        var _Result = await this.ApiAccess.SendRequestAsync<DuplicateShoppingListWebAppRequest, DuplicateShoppingListWebAppResponse>(
            new DuplicateShoppingListWebAppRequest() { Name = NameForCopy(this.m_ManageName) },
            ApiProvider.DuplicateShoppingList(this.m_ManagingListID.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Duplicating = false;

        if (_Result == null)
            return;

        this.m_ShowManage = false;

        await this.LoadListsAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);

        this.NavigationManager.NavigateTo($"/shopping-lists/{_Result.ShoppingListID}");
    }

    /// <summary>
    /// "Weekly shop" becomes "Weekly shop copy", and copying that gives "Weekly shop copy 2" rather
    /// than "Weekly shop copy copy".
    /// </summary>
    private static string NameForCopy(string name)
    {
        var _Name = name.Trim();
        var _CopyIndex = _Name.LastIndexOf(" copy", StringComparison.OrdinalIgnoreCase);

        if (_CopyIndex < 0)
            return $"{_Name} copy";

        var _Suffix = _Name[(_CopyIndex + 5)..].Trim();

        return _Suffix.Length == 0
            ? $"{_Name} 2"
            : int.TryParse(_Suffix, out var _Number)
                ? $"{_Name[.._CopyIndex]} copy {_Number + 1}"
                : $"{_Name} copy";
    }

    private async Task DeleteManagedListAsync()
    {
        if (this.m_Deleting || this.m_ManagingListID == null)
            return;

        if (!this.m_ConfirmingDelete)
        {
            this.m_ConfirmingDelete = true;
            return;
        }

        this.m_Deleting = true;

        var _Deleted = await this.DeleteShoppingListAsync(this.m_ManagingListID.Value);

        this.m_Deleting = false;

        if (_Deleted)
            this.m_ShowManage = false;
    }

    private async Task<bool> DeleteShoppingListAsync(long shoppingListID)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteShoppingList(shoppingListID),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != true)
            return false;

        if (this.ShoppingListID == shoppingListID)
            this.NavigationManager.NavigateTo("/shopping-lists");

        await this.LoadListsAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);

        return true;
    }

    #endregion Methods

}
