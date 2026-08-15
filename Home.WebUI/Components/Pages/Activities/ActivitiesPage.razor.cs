using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.Activities.CreateActivity;
using Home.WebUI.DataAccess.Activities.GetActivities;
using Home.WebUI.DataAccess.Activities.Models;
using Home.WebUI.DataAccess.Activities.SetActivityCompletion;
using Home.WebUI.DataAccess.Activities.SetActivityTags;
using Home.WebUI.DataAccess.Activities.UpdateActivity;
using Home.WebUI.DataAccess.ActivityStates.GetActivityStates;
using Home.WebUI.DataAccess.Tags.GetTags;
using Home.WebUI.DataAccess.Tags.Models;
using Home.WebUI.DataAccess.Users.GetUsers;
using Home.WebUI.DataAccess.Users.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Activities;

public partial class ActivitiesPage : IDisposable
{

    #region Fields

    private const string BoardView = "board";
    private const string DayView = "day";
    private const string WeekView = "week";
    private const string DefaultDueTime = "18:00";

    private static readonly List<HomeSegmentedControl<string>.SegmentOption> ViewOptions =
    [
        new("Board", BoardView),
        new("Week", WeekView),
        new("Day", DayView),
    ];

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;

    private List<ActivityStateDto> m_States = [];
    private List<ActivitySummaryDto> m_Activities = [];
    private List<TagDto> m_Tags = [];
    private List<UserSummaryDto> m_Users = [];
    private bool m_Loaded;

    private string m_View = BoardView;
    private DateTime m_Anchor;
    private DateTime m_Today;

    private bool m_Saving;
    private bool m_ShowBoardSettings;

    // Create
    private bool m_ShowCreate;
    private string m_NewTitle = string.Empty;
    private DateTime? m_NewDueDate;
    private bool m_NewHasTime;
    private string m_NewDueTime = DefaultDueTime;
    private long? m_NewStateID;
    private long? m_NewUserID;
    private HashSet<long> m_NewTagIDs = [];

    // Edit
    private bool m_ShowEdit;
    private ActivitySummaryDto? m_EditActivity;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        this.m_Today = this.TimeProvider.GetLocalNow().Date;
        this.m_Anchor = this.m_Today;

        await Task.WhenAll(
            this.LoadStatesAsync(),
            this.LoadActivitiesAsync(),
            this.LoadTagsAsync(),
            this.LoadUsersAsync());

        this.m_Loaded = true;

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
        if (area != ChangeArea.Activities && area != ChangeArea.Users)
            return;

        await this.InvokeAsync(async () =>
        {
            await Task.WhenAll(
                this.LoadStatesAsync(),
                this.LoadActivitiesAsync(),
                this.LoadTagsAsync(),
                this.LoadUsersAsync());

            this.StateHasChanged();
        });
    }

    // Loading

    private async Task LoadStatesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetActivityStatesWebAppResponse>(
            null!, ApiProvider.GetActivityStates(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_States = [.. _Result.States.OrderBy(s => s.Sequence)];
    }

    private async Task LoadActivitiesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetActivitiesWebAppResponse>(
            null!, ApiProvider.GetActivities(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Activities = [.. _Result.Activities];
    }

    private async Task LoadTagsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetTagsWebAppResponse>(
            null!, ApiProvider.GetTags(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Tags = [.. _Result.Tags.OrderBy(t => t.Name)];
    }

    private async Task LoadUsersAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetUsersWebAppResponse>(
            null!, ApiProvider.GetUsers(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Users = [.. _Result.Users];
    }

    private async Task ReloadAndPublishAsync()
    {
        await this.LoadStatesAsync();
        await this.LoadActivitiesAsync();
        await this.LoadTagsAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Activities, this.m_CancellationTokenHandler.Token);
    }

    // Views

    private void SelectView(string view)
    {
        this.m_View = view;
        this.m_Anchor = this.m_Today;
    }

    private void ShiftAnchor(int direction)
        => this.m_Anchor = this.m_Anchor.AddDays(direction * (this.m_View == WeekView ? 7 : 1));

    private void GoToToday()
        => this.m_Anchor = this.m_Today;

    private string GetViewTitle()
        => this.m_View switch
        {
            WeekView => "The week",
            DayView => "The day",
            _ => "The board"
        };

    private List<DateTime> GetDays()
    {
        if (this.m_View == DayView)
            return [this.m_Anchor.Date];

        var _StartOfWeek = ActivityBoardLogic.StartOfWeek(this.m_Anchor);

        return [.. Enumerable.Range(0, 7).Select(i => _StartOfWeek.AddDays(i))];
    }

    private string GetPeriodLabel()
    {
        if (this.m_View == DayView)
            return ActivityBoardLogic.DescribeLongDay(this.m_Anchor);

        var _StartOfWeek = ActivityBoardLogic.StartOfWeek(this.m_Anchor);

        return $"{ActivityBoardLogic.DescribeDate(_StartOfWeek)} – {ActivityBoardLogic.DescribeDate(_StartOfWeek.AddDays(6))}";
    }

    // Moving

    private async Task OnMoveAsync(ActivityMove move)
    {
        if (this.m_Saving) return;
        this.m_Saving = true;

        // Only the column. Anything else sent here would overwrite whatever another family
        // member changed on their own device since this board last loaded.
        var _Request = new UpdateActivityWebAppRequest()
        {
            StateID = new PropertyChangeTracker<long?>(move.StateID)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateActivityWebAppRequest, bool>(
            _Request, ApiProvider.UpdateActivity(move.Activity.ActivityID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Saving = false;

        if (_Result == true)
            await this.ReloadAndPublishAsync();
    }

    private async Task OnToggleCompleteAsync(ActivityCompletion completion)
    {
        if (this.m_Saving) return;
        this.m_Saving = true;

        var _Result = await this.ApiAccess.SendRequestAsync<SetActivityCompletionWebAppRequest, bool>(
            new SetActivityCompletionWebAppRequest() { IsComplete = completion.IsComplete },
            ApiProvider.SetActivityCompletion(completion.Activity.ActivityID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Saving = false;

        if (_Result == true)
            await this.ReloadAndPublishAsync();
    }

    private void OpenActivity(ActivitySummaryDto activity)
        => this.NavigationManager.NavigateTo($"/activities/{activity.ActivityID}");

    // Create

    private void OpenCreateModal()
    {
        this.m_NewTitle = string.Empty;
        this.m_NewDueDate = this.m_View == BoardView ? null : this.m_Anchor.Date;
        this.m_NewHasTime = false;
        this.m_NewDueTime = DefaultDueTime;
        this.m_NewStateID = this.m_States.FirstOrDefault()?.ActivityStateID;
        this.m_NewUserID = null;
        this.m_NewTagIDs = [];
        this.m_ShowCreate = true;
    }

    private async Task CreateActivityAsync()
    {
        if (this.m_Saving) return;
        if (string.IsNullOrWhiteSpace(this.m_NewTitle)) return;
        this.m_Saving = true;

        var _Request = new CreateActivityWebAppRequest()
        {
            DueDateUTC = this.m_NewDueDate,
            DueTime = ResolveDueTime(this.m_NewDueDate, this.m_NewHasTime, this.m_NewDueTime),
            StateID = this.m_NewStateID,
            Title = this.m_NewTitle,
            UserID = this.m_NewUserID
        };

        var _Response = await this.ApiAccess.SendRequestAsync<CreateActivityWebAppRequest, CreateActivityWebAppResponse>(
            _Request, ApiProvider.CreateActivity(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Response != null && this.m_NewTagIDs.Count > 0)
            _ = await this.SaveTagsAsync(_Response.ActivityID, this.m_NewTagIDs);

        this.m_Saving = false;

        if (_Response == null) return;

        this.m_ShowCreate = false;
        await this.ReloadAndPublishAsync();
    }

    private void ToggleNewTag(long tagID)
    {
        if (!this.m_NewTagIDs.Add(tagID))
            _ = this.m_NewTagIDs.Remove(tagID);
    }

    private async Task<bool> SaveTagsAsync(long activityID, IEnumerable<long> tagIDs)
    {
        var _Request = new SetActivityTagsWebAppRequest()
        {
            TagIDs = [.. tagIDs]
        };

        var _Result = await this.ApiAccess.SendRequestAsync<SetActivityTagsWebAppRequest, bool>(
            _Request, ApiProvider.SetActivityTags(activityID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        return _Result == true;
    }

    // Edit

    private void OpenEditModal(ActivitySummaryDto activity)
    {
        this.m_EditActivity = activity;
        this.m_ShowEdit = true;
    }

    private void OpenEditedActivity()
    {
        if (this.m_EditActivity == null)
            return;

        this.m_ShowEdit = false;
        this.OpenActivity(this.m_EditActivity);
    }

    // Helpers

    /// <summary>
    /// A time without a day is meaningless on a board, so clearing the date clears the time too.
    /// </summary>
    private static TimeSpan? ResolveDueTime(DateTime? dueDate, bool hasTime, string time)
        => dueDate.HasValue && hasTime ? ActivityBoardLogic.ParseTime(time) : null;

    #endregion Methods

}
