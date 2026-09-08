using Home.WebUI.Components.Pages.Calendar.Enumerations;
using Home.WebUI.Components.Pages.Calendar.Models;
using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.Activities.SetActivityCompletion;
using Home.WebUI.DataAccess.Calendar.CreateCalendarEvent;
using Home.WebUI.DataAccess.Calendar.GetCalendar;
using Home.WebUI.DataAccess.Calendar.GetCalendarEvent;
using Home.WebUI.DataAccess.Calendar.Models;
using Home.WebUI.DataAccess.Users.GetUsers;
using Home.WebUI.DataAccess.Users.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.Calendar;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;

namespace Home.WebUI.Components.Pages.Calendar;

public partial class CalendarPage : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;

    // Where and how we are looking
    private readonly List<HomeSegmentedControl<CalendarView>.SegmentOption> m_ViewOptions =
    [
        new("Month", CalendarView.Month),
        new("Week", CalendarView.Week),
        new("List", CalendarView.List)
    ];

    private DateOnly m_Anchor;
    private List<CalendarDayDto>? m_Days;
    private long? m_FilterUserID;
    private List<UserSummaryDto> m_Members = [];
    private string m_TimeZoneID = "UTC";
    private DateOnly m_Today;
    private CalendarView m_View = CalendarView.Week;

    // The tapped item and what is being done to it
    private bool m_Busy;
    private CalendarItemDto? m_SelectedItem;

    // The editor
    private CalendarItemDto? m_DetachingItem;
    private long? m_EditingEventID;
    private string? m_EditorProblem;
    private string m_EditorTitle = "New event";
    private CalendarEventForm m_Form = new();
    private bool m_Saving;
    private bool m_ShowEditor;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        this.m_TimeZoneID = await this.ViewerClock.GetTimeZoneIDAsync(this.m_CancellationTokenHandler.Token);
        this.m_Today = await this.ViewerClock.TodayAsync(this.m_CancellationTokenHandler.Token);
        this.m_Anchor = this.m_Today;

        await Task.WhenAll(this.LoadAsync(), this.LoadMembersAsync());

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);
    }

    public void Dispose()
    {
        this.m_ChangeSubscription?.Dispose();
        this.m_CancellationTokenHandler.Dispose();
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        // Meals and chores are on the calendar too, and a renamed member or recipe renames rows.
        if (area is not (ChangeArea.Calendar or ChangeArea.MealPlan or ChangeArea.Activities or ChangeArea.Users or ChangeArea.Recipes))
            return;

        await this.InvokeAsync(async () =>
        {
            await Task.WhenAll(this.LoadAsync(), area == ChangeArea.Users ? this.LoadMembersAsync() : Task.CompletedTask);
            this.StateHasChanged();
        });
    }

    // The window on show

    private (DateOnly From, DateOnly To) Range()
    {
        if (this.m_View == CalendarView.Month)
        {
            var _Start = CalendarFormat.StartOfWeek(this.FirstOfMonth());
            return (_Start, _Start.AddDays(41));
        }

        var _WeekStart = CalendarFormat.StartOfWeek(this.m_Anchor);
        return (_WeekStart, _WeekStart.AddDays(6));
    }

    private DateOnly FirstOfMonth()
        => new(this.m_Anchor.Year, this.m_Anchor.Month, 1);

    private async Task LoadAsync()
    {
        var (_From, _To) = this.Range();

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetCalendarWebAppResponse>(
            null!, ApiProvider.GetCalendar(_From, _To, this.m_TimeZoneID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Days = _Result.Days;
    }

    private async Task MoveAsync(int direction)
    {
        this.m_Anchor = this.m_View == CalendarView.Month
            ? this.FirstOfMonth().AddMonths(direction)
            : this.m_Anchor.AddDays(7 * direction);
        this.m_Days = null;

        await this.LoadAsync();
    }

    private async Task GoToTodayAsync()
    {
        this.m_Anchor = this.m_Today;
        this.m_Days = null;

        await this.LoadAsync();
    }

    private async Task ChangeViewAsync(CalendarView view)
    {
        if (view == this.m_View)
            return;

        this.m_View = view;
        this.m_Days = null;

        await this.LoadAsync();
    }

    /// <summary>
    /// A month cell with more on it than it can show opens as that week's list.
    /// </summary>
    private async Task ShowDayAsync(DateOnly date)
    {
        this.m_Anchor = date;
        this.m_View = CalendarView.List;
        this.m_Days = null;

        await this.LoadAsync();
    }

    /// <summary>
    /// The days as filtered by person. An item that names nobody is the household's and stays;
    /// one that names people stays only if the chosen member is among them.
    /// </summary>
    private List<CalendarDayDto> VisibleDays()
        => this.m_FilterUserID is { } _UserID && this.m_Days != null
            ? [.. this.m_Days.Select(d => new CalendarDayDto()
            {
                Date = d.Date,
                Items = [.. d.Items.Where(i => i.PersonUserIDs.Count == 0 || i.PersonUserIDs.Contains(_UserID))]
            })]
            : this.m_Days ?? [];

    private bool IsShowingToday()
    {
        var (_From, _To) = this.Range();
        return this.m_Today >= _From && this.m_Today <= _To;
    }

    private string NextLabel()
        => this.m_View == CalendarView.Month ? "Next month" : "Next week";

    private string PreviousLabel()
        => this.m_View == CalendarView.Month ? "Previous month" : "Previous week";

    private string RangeLabel()
        => this.m_View == CalendarView.Month
            ? CalendarFormat.MonthLabel(this.FirstOfMonth())
            : CalendarFormat.WeekLabel(CalendarFormat.StartOfWeek(this.m_Anchor));

    // Opening things

    private void SelectItem(CalendarItemDto item)
        => this.m_SelectedItem = item;

    private async Task OpenCreateAsync(DateOnly? date)
    {
        this.m_Form = CalendarEventForm.ForNew(date ?? this.m_Today);
        this.m_EditingEventID = null;
        this.m_DetachingItem = null;
        this.m_EditorProblem = null;
        this.m_EditorTitle = "New event";
        this.m_ShowEditor = true;

        await this.LoadMembersAsync();
    }

    /// <summary>
    /// Editing the whole series loads the rule as stored. Editing this time only detaches the
    /// occurrence: the editor opens on a one-off copy, and saving it also skips the original, so
    /// the series stands and this one day does what was asked (see the 8 Sep 2026 decision).
    /// </summary>
    private async Task EditSelectedAsync(bool wholeSeries)
    {
        if (this.m_SelectedItem is not { } _Item)
            return;

        var _Event = await this.ApiAccess.SendRequestAsync<object, GetCalendarEventWebAppResponse>(
            null!, ApiProvider.GetCalendarEvent(_Item.ID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Event == null)
            return;

        if (wholeSeries || !_Item.IsRecurring)
        {
            this.m_Form = CalendarEventForm.FromEvent(_Event);
            this.m_EditingEventID = _Event.CalendarEventID;
            this.m_DetachingItem = null;
            this.m_EditorTitle = _Item.IsRecurring ? "Edit the series" : "Edit event";
        }
        else
        {
            this.m_Form = CalendarEventForm.FromOccurrence(_Item, _Event);
            this.m_EditingEventID = null;
            this.m_DetachingItem = _Item;
            this.m_EditorTitle = $"Just {CalendarFormat.ShortDate(_Item.Date)}";
        }

        this.m_EditorProblem = null;
        this.m_SelectedItem = null;
        this.m_ShowEditor = true;

        await this.LoadMembersAsync();
    }

    private async Task LoadMembersAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetUsersWebAppResponse>(
            null!, ApiProvider.GetUsers(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Members = [.. _Result.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)];
    }

    // Saving and removing

    private async Task SaveEditorAsync()
    {
        if (this.m_Saving)
            return;

        this.m_EditorProblem = this.m_Form.Problem();

        if (this.m_EditorProblem != null)
            return;

        this.m_Saving = true;

        var _Request = this.m_Form.ToRequest(this.m_TimeZoneID);
        var _Saved = false;

        if (this.m_EditingEventID is { } _EventID)
        {
            _Saved = await this.ApiAccess.SendRequestAsync<CalendarEventWebAppRequest, bool>(
                _Request, ApiProvider.UpdateCalendarEvent(_EventID),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token) == true;
        }
        else
        {
            var _Created = await this.ApiAccess.SendRequestAsync<CalendarEventWebAppRequest, CreateCalendarEventWebAppResponse>(
                _Request, ApiProvider.CreateCalendarEvent(),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);

            _Saved = _Created != null;

            // The copy exists, so the original occurrence can go. The other way round would lose
            // the day entirely if the create failed.
            if (_Saved && this.m_DetachingItem is { } _Detaching)
                _ = await this.ApiAccess.SendRequestAsync<object, bool>(
                    null!, ApiProvider.DeleteCalendarEvent(_Detaching.ID, _Detaching.OccurrenceDate ?? _Detaching.Date),
                    e => this.m_ErrorHandler?.AddError(e),
                    this.m_CancellationTokenHandler.Token);
        }

        this.m_Saving = false;

        if (!_Saved)
            return;

        this.m_ShowEditor = false;
        this.m_DetachingItem = null;

        await this.LoadAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Calendar, this.m_CancellationTokenHandler.Token);
    }

    private async Task DeleteSelectedAsync(bool wholeSeries)
    {
        if (this.m_Busy || this.m_SelectedItem is not { } _Item)
            return;

        this.m_Busy = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteCalendarEvent(_Item.ID, wholeSeries ? null : _Item.OccurrenceDate ?? _Item.Date),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Busy = false;

        if (_Result != true)
            return;

        this.m_SelectedItem = null;

        await this.LoadAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Calendar, this.m_CancellationTokenHandler.Token);
    }

    private async Task CompleteSelectedTaskAsync()
    {
        if (this.m_Busy || this.m_SelectedItem is not { } _Item)
            return;

        this.m_Busy = true;

        var _Result = await this.ApiAccess.SendRequestAsync<SetActivityCompletionWebAppRequest, bool>(
            new SetActivityCompletionWebAppRequest() { IsComplete = true },
            ApiProvider.SetActivityCompletion(_Item.ID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Busy = false;

        if (_Result != true)
            return;

        this.m_SelectedItem = null;

        await this.LoadAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Activities, this.m_CancellationTokenHandler.Token);
    }

    #endregion Methods

}
