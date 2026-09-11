using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.Components.Pages.ShoppingList.Enumerations;
using Home.WebUI.Components.Pages.ShoppingList.Models;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.Recipes.Models;
using Home.WebUI.DataAccess.ShoppingCategories.GetShoppingCategories;
using Home.WebUI.DataAccess.ShoppingCategories.Models;
using Home.WebUI.DataAccess.ShoppingListItems.CreateShoppingListItem;
using Home.WebUI.DataAccess.ShoppingListItems.GetShoppingListItemSuggestions;
using Home.WebUI.DataAccess.ShoppingListItems.SetShoppingListItemCategory;
using Home.WebUI.DataAccess.ShoppingListItems.UpdateShoppingListItem;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.Models;
using Home.WebUI.DataAccess.ShoppingLists.UpdateShoppingList;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Home.WebUI.Infrastructure.Services.ShoppingLists;
using Home.WebUI.Infrastructure.ShoppingLists;
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
    private List<ShoppingCategoryDto> m_Aisles = [];
    private bool m_ShowAisles;

    private HomeTextInput? m_QuickAddInput;
    private string m_QuickAddText = string.Empty;
    private bool m_AddingItem;
    private bool m_ShowSuggestions;

    /// <summary>
    /// Long enough for the tap that chose a suggestion to reach it, short enough that the list is
    /// gone before anyone notices it lingering.
    /// </summary>
    private static readonly TimeSpan s_SuggestionCloseDelay = TimeSpan.FromMilliseconds(150);

    private int m_SuggestionCloseToken;
    private List<GetShoppingListItemSuggestionDto> m_Suggestions = [];

    private bool m_ShowTrolley;
    private bool m_RunningListAction;
    private bool m_ShowConfirmClear;

    private bool m_ShowEditItem;
    private long? m_EditingItemID;
    private string m_EditName = string.Empty;
    private string m_EditAmount = string.Empty;
    private string m_EditCost = string.Empty;
    private long? m_EditUnit;

    /// <summary>
    /// Pieces is left out: it is the one unit with nothing to show beside a number, which is what
    /// "Just a number" already says.
    /// </summary>
    private static readonly List<HomeSelect<long?>.SelectOption> UnitOptions =
    [
        new("Just a number", null),
        .. MeasurementUnits.All
            .Where(u => u.Abbreviation.Length > 0)
            .Select(u => new HomeSelect<long?>.SelectOption(u.Name, u.Value))
    ];

    private static readonly List<HomeSegmentedControl<bool>.SegmentOption> ViewOptions =
    [
        new("My order", false),
        new("By aisle", true)
    ];

    private string m_EditNote = string.Empty;
    private long? m_EditAisleID;
    private long? m_EditOriginalAisleID;
    private string m_EditOriginalName = string.Empty;
    private string? m_EditUsualHint;
    private bool m_SavingItem;
    private bool m_Reordering;

    /// <summary>
    /// What is being dragged. Only a pointer device ever sets this: touch fires no drag events at
    /// all, which is why the reorder chevrons on each row are the primary way to move something.
    /// </summary>
    private ShoppingListItemDto? m_DraggedItem;

    /// <summary>
    /// The row the cursor is currently over, which is where the line is drawn.
    /// </summary>
    private ShoppingListItemDto? m_DragOverItem;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [CascadingParameter] public ShoppingListDrag? Drag { get; set; }
    [Parameter] public long? ShoppingListID { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.CancellationToken);

        // Mutating a cascaded object tells Blazor nothing, so the drag says when it changed. This
        // pane needs it to put its drop line away the moment the cursor reaches the picker.
        if (this.Drag != null)
            this.Drag.Changed += this.OnDragChanged;

        await this.LoadAislesAsync();
        await this.LoadSuggestionsAsync();
    }

    private void OnDragChanged()
        => _ = this.InvokeAsync(this.StateHasChanged);

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
    {
        if (this.Drag != null)
            this.Drag.Changed -= this.OnDragChanged;

        this.m_ChangeSubscription?.Dispose();
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        if (area != ChangeArea.ShoppingLists || !this.ShoppingListID.HasValue)
            return;

        await this.InvokeAsync(async () =>
        {
            await this.LoadAislesAsync();
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

    private async Task LoadAislesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetShoppingCategoriesWebAppResponse>(
            null!, ApiProvider.GetShoppingCategories(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != null)
            this.m_Aisles = [.. _Result.ShoppingCategories];
    }

    /// <summary>
    /// A renamed, moved or removed aisle changes how every list reads, so other phones are told too.
    /// </summary>
    private async Task OnAislesChangedAsync()
    {
        await this.LoadAislesAsync();

        if (this.ShoppingListID.HasValue)
            await this.LoadListAsync();

        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    /// <summary>
    /// Switched on screen straight away and saved behind it, like a tick.
    /// </summary>
    private async Task SetGroupByAisleAsync(bool groupByAisle)
    {
        if (this.m_ShoppingList == null || !this.ShoppingListID.HasValue || this.m_ShoppingList.GroupByAisle == groupByAisle)
            return;

        this.m_ShoppingList.GroupByAisle = groupByAisle;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateShoppingListWebAppRequest, bool>(
            new UpdateShoppingListWebAppRequest() { GroupByAisle = new(groupByAisle) },
            ApiProvider.UpdateShoppingList(this.ShoppingListID.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != true)
        {
            await this.LoadListAsync();
            return;
        }

        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
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
    /// Opening the list again cancels any close that was waiting to happen.
    /// </summary>
    private void OnQuickAddFocus()
    {
        this.m_SuggestionCloseToken++;
        this.m_ShowSuggestions = true;
    }

    /// <summary>
    /// Closes the suggestion list, but not straight away. A tap on a suggestion blurs the text box
    /// before it reaches the button, so a list that closes on blur removes the thing being tapped
    /// and the tap lands on nothing. The desktop answer to this is the preventDefault on the
    /// button's mousedown, which keeps focus in the box, and which a touch browser is entitled to
    /// ignore because there was no mouse. Waiting instead does not depend on that.
    /// <para>
    /// The token is what makes it safe: anything that reopens the list, or closes it deliberately,
    /// moves the token on and this close does nothing when it wakes up.
    /// </para>
    /// </summary>
    private async Task OnQuickAddBlurAsync()
    {
        var _Token = ++this.m_SuggestionCloseToken;

        try
        {
            await Task.Delay(s_SuggestionCloseDelay, this.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (_Token != this.m_SuggestionCloseToken)
            return;

        this.m_ShowSuggestions = false;

        this.StateHasChanged();
    }

    /// <summary>
    /// An amount already typed beats the one it was last bought with. Someone who wrote "2 kg pot"
    /// and then picked Potatoes wants two kilos, not whatever last week's shop had.
    /// </summary>
    private async Task AddSuggestionAsync(GetShoppingListItemSuggestionDto suggestion)
    {
        // Chosen, so the pending close from the blur that came with the tap must not fire.
        this.m_SuggestionCloseToken++;

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
        this.m_EditAmount = item.Amount?.ToString("0.##") ?? string.Empty;
        this.m_EditCost = item.Cost?.ToString("0.00") ?? string.Empty;
        this.m_EditUnit = item.Unit;
        this.m_EditNote = item.Note ?? string.Empty;
        this.m_EditAisleID = item.ShoppingCategoryID;
        this.m_EditOriginalAisleID = item.ShoppingCategoryID;
        this.m_EditOriginalName = item.Name;
        this.m_EditUsualHint = DescribeUsual(item);
        this.m_ShowEditItem = true;
    }

    /// <summary>
    /// Said for the amount the line was saved with, because that is the amount usual was worked out
    /// for.
    /// </summary>
    private static string? DescribeUsual(ShoppingListItemDto item)
    {
        if (item.UsualCost is not { } _Usual)
            return null;

        var _Amount = ShoppingListItemLogic.DescribeAmount(item);

        return _Amount.Length == 0 ? $"Usually ${_Usual:F2}" : $"Usually ${_Usual:F2} for {_Amount}";
    }

    /// <summary>
    /// Swaps an item with its neighbour among the things still to get, so the list can be put in
    /// the order the shop is walked. Ticked items keep their place and are not reorderable.
    /// </summary>
    private async Task MoveItemAsync(ShoppingListItemDto item, int direction)
    {
        var _ToGet = this.ItemsToGet().ToList();
        var _Index = _ToGet.FindIndex(i => i.ShoppingListItemID == item.ShoppingListItemID);
        var _TargetIndex = _Index + direction;

        if (_Index < 0 || _TargetIndex < 0 || _TargetIndex >= _ToGet.Count)
            return;

        await this.MoveItemToAsync(item, _ToGet[_TargetIndex]);
    }

    /// <summary>
    /// Puts one item where another one is. One call: the API takes it out of the order and puts it
    /// back at that position, closing the gap behind it. This used to be a pair of calls swapping
    /// two sequences, which only ever worked for neighbours and is not what a drop onto a row four
    /// places away means.
    /// </summary>
    private async Task MoveItemToAsync(ShoppingListItemDto item, ShoppingListItemDto target)
    {
        if (this.m_Reordering || item.ShoppingListItemID == target.ShoppingListItemID)
            return;

        this.m_Reordering = true;

        var _Moved = await this.SetItemSequenceAsync(item, target.Sequence);

        this.m_Reordering = false;

        if (!_Moved)
            return;

        await this.LoadListAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    private void StartDraggingItem(ShoppingListItemDto item)
    {
        this.m_DraggedItem = item;
        this.m_DragOverItem = null;

        // Also published to the page, so the lists pane knows what would land on it.
        this.Drag?.Start(item, this.ShoppingListID);
    }

    /// <summary>
    /// The drag ended without a drop landing anywhere useful, so every trace of it goes.
    /// </summary>
    private void EndDraggingItem()
    {
        this.m_DraggedItem = null;
        this.m_DragOverItem = null;

        this.Drag?.Clear();
    }

    private void DragOverItem(ShoppingListItemDto item)
    {
        this.m_DragOverItem = item;

        // Back off the picker and onto the items, so the list highlight lets go.
        this.Drag?.LeaveLists();
    }

    /// <summary>
    /// Which edge of a row gets the line. Dropping onto a row takes that row's position, so an item
    /// travelling down the list ends up below the row it lands on and one travelling up ends up
    /// above it. Drawing the line on the nearest edge instead would promise the wrong result half
    /// the time.
    /// </summary>
    private ShoppingListDropLine DropLineFor(ShoppingListItemDto item, IReadOnlyList<ShoppingListItemDto> toGet)
    {
        // Over a list in the picker, so that is where it would land and the line here would be
        // promising somewhere else.
        if (this.Drag?.OverShoppingListID != null)
            return ShoppingListDropLine.None;

        if (this.m_DraggedItem is not { } _Dragged
            || this.m_DragOverItem?.ShoppingListItemID != item.ShoppingListItemID
            || _Dragged.ShoppingListItemID == item.ShoppingListItemID)
            return ShoppingListDropLine.None;

        var _From = toGet.ToList().FindIndex(i => i.ShoppingListItemID == _Dragged.ShoppingListItemID);
        var _To = toGet.ToList().FindIndex(i => i.ShoppingListItemID == item.ShoppingListItemID);

        if (_From < 0 || _To < 0)
            return ShoppingListDropLine.None;

        return _From < _To ? ShoppingListDropLine.Below : ShoppingListDropLine.Above;
    }

    /// <summary>
    /// A row was dropped on. Only meaningful for something dragged from this same list; the lists
    /// pane handles a drag that lands on a different list.
    /// </summary>
    private async Task DropOnItemAsync(ShoppingListItemDto target)
    {
        if (this.m_DraggedItem is not { } _Dragged)
            return;

        this.m_DraggedItem = null;
        this.m_DragOverItem = null;
        this.Drag?.Clear();

        await this.MoveItemToAsync(_Dragged, target);
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
                Note = new(this.m_EditNote),
                ShoppingListItemID = this.m_EditingItemID.Value,
                Unit = new(this.m_EditUnit)
            },
            ApiProvider.UpdateShoppingListItem(this.m_EditingItemID.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result == true && this.AisleNeedsFiling(_Name))
        {
            _Result = await this.ApiAccess.SendRequestAsync<SetShoppingListItemCategoryWebAppRequest, bool>(
                new SetShoppingListItemCategoryWebAppRequest() { ShoppingCategoryID = this.m_EditAisleID },
                ApiProvider.SetShoppingListItemCategory(this.m_EditingItemID.Value),
                e => this.m_ErrorHandler?.AddError(e),
                this.CancellationToken);
        }

        this.m_SavingItem = false;

        if (_Result != true)
            return;

        this.m_ShowEditItem = false;

        await this.LoadListAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.CancellationToken);
    }

    /// <summary>
    /// An aisle is filed against the item's name, so a renamed item is filed again under whatever
    /// the sheet shows, even when the choice itself did not change.
    /// </summary>
    private bool AisleNeedsFiling(string name)
        => this.m_EditAisleID != this.m_EditOriginalAisleID
            || (this.m_EditAisleID != null && !string.Equals(name, this.m_EditOriginalName, StringComparison.OrdinalIgnoreCase));

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
    /// Both of these are one call rather than one per item, because a thirty-line list emptying a line at
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

    private ShoppingListEstimate ListEstimate()
        => this.AisleLogic.Estimate(this.m_ShoppingList?.Items ?? []);

    private static string DescribeTotal(ShoppingListEstimate estimate)
        => estimate.IncludesGuesses ? $"About ${estimate.Total:F2}" : $"${estimate.Total:F2}";

    private decimal TrolleyTotal()
        => (this.m_ShoppingList?.Items ?? []).Where(i => i.InBasket).Sum(i => i.Cost ?? 0);

    private bool IsGroupedByAisle()
        => this.m_ShoppingList?.GroupByAisle == true;

    private List<HomeSelect<long?>.SelectOption> AisleOptions()
        => [new("No aisle", null), .. this.m_Aisles.Select(a => new HomeSelect<long?>.SelectOption(a.Name, a.ShoppingCategoryID))];

    /// <summary>
    /// Headings and rows share a parent and an aisle's ID can equal an item's, so a heading is keyed
    /// by a string that no row's number can match.
    /// </summary>
    private static string AisleKey(ShoppingAisleGroup group)
        => $"aisle-{group.Aisle?.ShoppingCategoryID}";

    #endregion Reading Methods

}
