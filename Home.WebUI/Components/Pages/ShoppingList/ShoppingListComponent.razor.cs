using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.ShoppingListItems.CreateShoppingListItem;
using Home.WebUI.DataAccess.ShoppingListItems.GetShoppingListItemSuggestions;
using Home.WebUI.DataAccess.ShoppingListItems.UpdateShoppingListItem;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingListComponent : IDisposable
{

    #region Fields

    /// <summary>
    /// Enough to recognise what you meant, few enough that the list underneath stays visible.
    /// </summary>
    private const int SuggestionsShown = 6;

    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;
    private GetShoppingListWebAppResponse? m_ShoppingList;
    private long? m_LoadedShoppingListID;
    private bool m_LoadingList;

    private HomeTextInput? m_QuickAddInput;
    private string m_QuickAddText = string.Empty;
    private bool m_AddingItem;
    private bool m_ShowSuggestions;
    private List<GetShoppingListItemSuggestionDto> m_Suggestions = [];

    private bool m_ShowTrolley;
    private bool m_RunningListAction;
    private bool m_ShowConfirmClear;

    private bool m_ShowEditItem;
    private long? m_EditingItemID;
    private string m_EditName = string.Empty;
    private string m_EditAmount = string.Empty;
    private string m_EditCost = string.Empty;
    private string m_EditUnit = string.Empty;
    private bool m_SavingItem;
    private bool m_Reordering;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [Parameter] public long? ShoppingListID { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.CancellationToken);

        await this.LoadSuggestionsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (this.ShoppingListID == this.m_LoadedShoppingListID)
            return;

        // Everything on screen belongs to the list we are leaving, including a half-typed line and
        // an open edit, so none of it may survive the switch.
        this.m_LoadedShoppingListID = this.ShoppingListID;
        this.m_ShoppingList = null;
        this.m_QuickAddText = string.Empty;
        this.m_ShowSuggestions = false;
        this.m_ShowTrolley = false;
        this.m_ShowEditItem = false;
        this.m_ShowConfirmClear = false;
        this.m_EditingItemID = null;

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

    /// <summary>
    /// Fetched once and filtered on the device. What a household usually buys does not change
    /// between keystrokes, and a phone in a supermarket should not be asking the server on each
    /// letter typed.
    /// </summary>
    private async Task LoadSuggestionsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetShoppingListItemSuggestionsWebAppResponse>(
            null!, ApiProvider.GetShoppingListItemSuggestions(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != null)
            this.m_Suggestions = [.. _Result.Suggestions];
    }

    #endregion Methods

    #region Adding Methods

    private async Task QuickAddAsync()
    {
        var _Parsed = ShoppingListItemLogic.Parse(this.m_QuickAddText);

        if (_Parsed.Name.Length == 0)
            return;

        await this.AddItemAsync(_Parsed.Amount, null, _Parsed.Name, _Parsed.Unit);
    }

    /// <summary>
    /// An amount already typed beats the one it was last bought with — someone who wrote "2 kg pot"
    /// and then picked Potatoes wants two kilos, not whatever last week's shop had.
    /// </summary>
    private async Task AddSuggestionAsync(GetShoppingListItemSuggestionDto suggestion)
    {
        var _Typed = ShoppingListItemLogic.Parse(this.m_QuickAddText);

        await this.AddItemAsync(
            _Typed.Amount ?? suggestion.Amount,
            suggestion.Cost,
            suggestion.Name,
            _Typed.Amount != null ? _Typed.Unit : suggestion.Unit);
    }

    private async Task AddItemAsync(decimal? amount, decimal? cost, string name, long? unit)
    {
        if (this.m_AddingItem || !this.ShoppingListID.HasValue)
            return;

        this.m_AddingItem = true;

        // The target list is read at submit time, never when the line was started.
        var _Request = new CreateShoppingListItemWebAppRequest()
        {
            Amount = amount,
            Cost = cost,
            Name = name,
            ShoppingListID = this.ShoppingListID.Value,
            Unit = unit
        };

        var _Result = await this.ApiAccess.SendRequestAsync<CreateShoppingListItemWebAppRequest, CreateShoppingListItemWebAppResponse>(
            _Request,
            ApiProvider.CreateShoppingListItem(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_AddingItem = false;

        if (_Result == null)
            return;

        this.m_QuickAddText = string.Empty;

        if (_Request.ShoppingListID != this.ShoppingListID)
            return;

        await this.LoadListAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);

        // Writing a list is one thing after another, so the cursor goes straight back where it was.
        if (this.m_QuickAddInput != null)
            await this.m_QuickAddInput.FocusAsync();
    }

    private async Task OnQuickAddKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await this.QuickAddAsync();
        else if (e.Key == "Escape")
            this.m_ShowSuggestions = false;
    }

    /// <summary>
    /// Matching on the parsed name rather than the raw text means "2 kg pot" still finds Potatoes.
    /// </summary>
    private IEnumerable<GetShoppingListItemSuggestionDto> VisibleSuggestions()
    {
        var _OnTheList = (this.m_ShoppingList?.Items ?? [])
            .Select(i => i.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var _Candidates = this.m_Suggestions.Where(s => !_OnTheList.Contains(s.Name));
        var _Typed = ShoppingListItemLogic.Parse(this.m_QuickAddText).Name;

        return _Typed.Length == 0
            ? _Candidates.Take(SuggestionsShown)
            : _Candidates
                .Where(s => s.Name.Contains(_Typed, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Name.StartsWith(_Typed, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenByDescending(s => s.TimesAdded)
                .Take(SuggestionsShown);
    }

    #endregion Adding Methods

    #region Item Methods

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

    private void OpenEditItem(ShoppingListItemDto item)
    {
        this.m_EditingItemID = item.ShoppingListItemID;
        this.m_EditName = item.Name;
        this.m_EditAmount = (item.Amount ?? item.Quantity)?.ToString("0.##") ?? string.Empty;
        this.m_EditCost = item.Cost?.ToString("0.00") ?? string.Empty;
        this.m_EditUnit = item.Unit?.ToString() ?? string.Empty;
        this.m_ShowEditItem = true;
    }

    /// <summary>
    /// Swaps an item with its neighbour among the things still to get, so the list can be put in
    /// the order the shop is walked. Ticked items keep their place and are not reorderable.
    /// </summary>
    private async Task MoveItemAsync(ShoppingListItemDto item, int direction)
    {
        if (this.m_Reordering)
            return;

        var _ToGet = this.ItemsToGet().ToList();
        var _Index = _ToGet.FindIndex(i => i.ShoppingListItemID == item.ShoppingListItemID);
        var _TargetIndex = _Index + direction;

        if (_Index < 0 || _TargetIndex < 0 || _TargetIndex >= _ToGet.Count)
            return;

        var _Target = _ToGet[_TargetIndex];
        this.m_Reordering = true;

        var _Moved = await this.SetItemSequenceAsync(item, _Target.Sequence);

        if (_Moved)
            _ = await this.SetItemSequenceAsync(_Target, item.Sequence);

        this.m_Reordering = false;

        if (!_Moved)
            return;

        await this.LoadListAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    /// <summary>
    /// Only the sequence is sent, so a reorder cannot overwrite a name or a price someone is
    /// editing on another device.
    /// </summary>
    private async Task<bool> SetItemSequenceAsync(ShoppingListItemDto item, long sequence)
    {
        return await this.ApiAccess.SendRequestAsync<UpdateShoppingListItemWebAppRequest, bool>(
            new UpdateShoppingListItemWebAppRequest()
            {
                Sequence = new(sequence),
                ShoppingListItemID = item.ShoppingListItemID
            },
            ApiProvider.UpdateShoppingListItem(item.ShoppingListItemID),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken) == true;
    }

    private async Task SaveItemAsync()
    {
        if (this.m_SavingItem || !this.m_EditingItemID.HasValue)
            return;

        var _Name = this.m_EditName.Trim();

        if (_Name.Length == 0)
            return;

        this.m_SavingItem = true;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateShoppingListItemWebAppRequest, bool>(
            new UpdateShoppingListItemWebAppRequest()
            {
                Amount = new(decimal.TryParse(this.m_EditAmount, out var _Amount) ? _Amount : null),
                Cost = new(decimal.TryParse(this.m_EditCost, out var _Cost) ? _Cost : null),
                Name = new(_Name),
                ShoppingListItemID = this.m_EditingItemID.Value,
                Unit = new(long.TryParse(this.m_EditUnit, out var _Unit) ? _Unit : null)
            },
            ApiProvider.UpdateShoppingListItem(this.m_EditingItemID.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_SavingItem = false;

        if (_Result != true)
            return;

        this.m_ShowEditItem = false;

        await this.LoadListAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    private async Task DeleteItemAsync()
    {
        if (this.m_SavingItem || !this.m_EditingItemID.HasValue)
            return;

        this.m_SavingItem = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteShoppingListItem(this.m_EditingItemID.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_SavingItem = false;

        if (_Result != true)
            return;

        this.m_ShowEditItem = false;

        await this.LoadListAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    #endregion Item Methods

    #region List Action Methods

    private Task UntickAllAsync()
        => this.RunListActionAsync(ApiProvider.UntickShoppingListItems(this.ShoppingListID!.Value));

    private async Task ClearTickedAsync()
    {
        this.m_ShowConfirmClear = false;

        await this.RunListActionAsync(ApiProvider.DeleteTickedShoppingListItems(this.ShoppingListID!.Value));
    }

    /// <summary>
    /// Both of these are one call rather than one per item — a thirty-line list emptying a line at
    /// a time over a supermarket connection is the difference between instant and painful.
    /// </summary>
    private async Task RunListActionAsync(ApiProviderHelper apiProvider)
    {
        if (this.m_RunningListAction || !this.ShoppingListID.HasValue)
            return;

        this.m_RunningListAction = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, apiProvider,
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_RunningListAction = false;

        if (_Result != true)
            return;

        await this.LoadListAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    #endregion List Action Methods

    #region Reading Methods

    private IEnumerable<ShoppingListItemDto> Items()
        => (this.m_ShoppingList?.Items ?? []).OrderBy(i => i.Sequence).ThenBy(i => i.ShoppingListItemID);

    private IEnumerable<ShoppingListItemDto> ItemsToGet()
        => this.Items().Where(i => !i.InBasket);

    private IEnumerable<ShoppingListItemDto> ItemsInTrolley()
        => this.Items().Where(i => i.InBasket);

    private int ItemCount()
        => this.m_ShoppingList?.Items.Count ?? 0;

    private int TrolleyCount()
        => this.m_ShoppingList?.Items.Count(i => i.InBasket) ?? 0;

    private int ProgressPercent()
        => this.ItemCount() == 0 ? 0 : (int)Math.Round(this.TrolleyCount() * 100d / this.ItemCount());

    /// <summary>
    /// A cost is what the line costs, not a price per kilo — multiplying it by an amount would
    /// turn "$3.50 for 2 kg of potatoes" into seven dollars.
    /// </summary>
    private decimal ListTotal()
        => (this.m_ShoppingList?.Items ?? []).Sum(i => i.Cost ?? 0);

    private decimal TrolleyTotal()
        => (this.m_ShoppingList?.Items ?? []).Where(i => i.InBasket).Sum(i => i.Cost ?? 0);

    #endregion Reading Methods

}
