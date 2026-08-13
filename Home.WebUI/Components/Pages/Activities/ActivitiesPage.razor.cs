using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Activities.CreateActivity;
using Home.WebUI.DataAccess.Activities.GetActivities;
using Home.WebUI.DataAccess.Activities.Models;
using Home.WebUI.DataAccess.Activities.UpdateActivity;
using Home.WebUI.DataAccess.ActivityStates.GetActivityStates;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Activities;

public partial class ActivitiesPage
{

    #region Records

    private record ColumnVm(string Title, long? StateID);

    #endregion Records

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;

    private List<ActivityStateDto>? m_States;
    private List<ActivitySummaryDto>? m_Activities;
    private ActivitySummaryDto? m_DraggedActivity;

    private bool m_Saving;

    // Create
    private bool m_ShowCreate;
    private string m_NewTitle = string.Empty;
    private DateTime? m_NewDueDate;
    private long? m_NewStateID;

    // Edit
    private bool m_ShowEdit;
    private ActivitySummaryDto? m_EditActivity;
    private string m_EditTitle = string.Empty;
    private DateTime? m_EditDueDate;
    private long? m_EditStateID;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        var _States = await this.ApiAccess.SendRequestAsync<object, GetActivityStatesWebAppResponse>(
            null!, ApiProvider.GetActivityStates(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_States != null)
            this.m_States = [.. _States.States];

        await this.LoadActivitiesAsync();
    }

    #endregion Lifecycle Methods

    #region Methods

    // Columns

    private List<ColumnVm> GetColumns()
    {
        var _Columns = new List<ColumnVm>();

        if (this.HasUnassigned())
            _Columns.Add(new ColumnVm("Unassigned", null));

        foreach (var _State in this.m_States ?? [])
            _Columns.Add(new ColumnVm(_State.Name, _State.ActivityStateID));

        return _Columns;
    }

    // Loading

    private async Task LoadActivitiesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetActivitiesWebAppResponse>(
            null!, ApiProvider.GetActivities(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Activities = [.. _Result.Activities];
    }

    private List<ActivitySummaryDto> ActivitiesForState(long? stateID)
    {
        if (this.m_Activities == null)
            return [];

        if (stateID == null)
            return [.. this.m_Activities.Where(a => a.StateID == null || !this.IsKnownState(a.StateID.Value))];

        return [.. this.m_Activities.Where(a => a.StateID == stateID)];
    }

    private bool IsKnownState(long stateID)
        => this.m_States?.Any(s => s.ActivityStateID == stateID) ?? false;

    private bool HasUnassigned()
        => this.m_Activities?.Any(a => a.StateID == null || !this.IsKnownState(a.StateID.Value)) ?? false;

    // Drag & drop

    private void OnDragStart(ActivitySummaryDto activity)
        => this.m_DraggedActivity = activity;

    private async Task OnDropAsync(long? stateID)
    {
        var _Activity = this.m_DraggedActivity;
        this.m_DraggedActivity = null;

        if (_Activity == null || _Activity.StateID == stateID)
            return;

        await this.MoveActivityAsync(_Activity, stateID);
    }

    private async Task MoveActivityAsync(ActivitySummaryDto activity, long? stateID)
    {
        var _Request = BuildUpdateRequest(activity);
        _Request.StateID = new PropertyChangeTracker<long?>(stateID);

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateActivityWebAppRequest, bool>(
            _Request, ApiProvider.UpdateActivity(activity.ActivityID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == true)
            await this.LoadActivitiesAsync();
    }

    // Create

    private void OpenCreateModal()
    {
        this.m_NewTitle = string.Empty;
        this.m_NewDueDate = null;
        this.m_NewStateID = this.m_States?.FirstOrDefault()?.ActivityStateID;
        this.m_ShowCreate = true;
    }

    private async Task CreateActivityAsync()
    {
        if (this.m_Saving) return;
        if (string.IsNullOrWhiteSpace(this.m_NewTitle)) return;
        this.m_Saving = true;

        var _Request = new CreateActivityWebAppRequest()
        {
            Title = this.m_NewTitle,
            DueDateUTC = this.m_NewDueDate,
            StateID = this.m_NewStateID
        };

        var _Response = await this.ApiAccess.SendRequestAsync<CreateActivityWebAppRequest, CreateActivityWebAppResponse>(
            _Request, ApiProvider.CreateActivity(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Saving = false;

        if (_Response == null) return;

        this.m_ShowCreate = false;
        await this.LoadActivitiesAsync();
    }

    // Edit

    private void OpenEditModal(ActivitySummaryDto activity)
    {
        this.m_EditActivity = activity;
        this.m_EditTitle = activity.Title;
        this.m_EditDueDate = activity.DueDateUTC?.Date;
        this.m_EditStateID = activity.StateID;
        this.m_ShowEdit = true;
    }

    private async Task SaveActivityAsync()
    {
        if (this.m_Saving || this.m_EditActivity == null) return;
        this.m_Saving = true;

        var _Request = BuildUpdateRequest(this.m_EditActivity);
        _Request.Title = new PropertyChangeTracker<string>(this.m_EditTitle);
        _Request.DueDateUTC = new PropertyChangeTracker<DateTime?>(this.m_EditDueDate);
        _Request.StateID = new PropertyChangeTracker<long?>(this.m_EditStateID);

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateActivityWebAppRequest, bool>(
            _Request, ApiProvider.UpdateActivity(this.m_EditActivity.ActivityID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Saving = false;

        if (_Result != true) return;

        this.m_ShowEdit = false;
        await this.LoadActivitiesAsync();
    }

    private async Task DeleteActivityAsync()
    {
        if (this.m_Saving || this.m_EditActivity == null) return;
        this.m_Saving = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteActivity(this.m_EditActivity.ActivityID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Saving = false;

        if (_Result != true) return;

        this.m_ShowEdit = false;
        await this.LoadActivitiesAsync();
    }

    // Helpers

    private static UpdateActivityWebAppRequest BuildUpdateRequest(ActivitySummaryDto activity)
        => new()
        {
            Title = new PropertyChangeTracker<string>(activity.Title),
            DueDateUTC = new PropertyChangeTracker<DateTime?>(activity.DueDateUTC),
            CompletedDateUTC = new PropertyChangeTracker<DateTime?>(activity.CompletedDateUTC),
            StateID = new PropertyChangeTracker<long?>(activity.StateID),
            StatusID = new PropertyChangeTracker<long?>(activity.StatusID),
            UserID = new PropertyChangeTracker<long?>(activity.AssignedToUserID)
        };

    #endregion Methods

}
