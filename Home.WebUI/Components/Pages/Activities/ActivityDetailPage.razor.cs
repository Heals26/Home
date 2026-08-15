using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Activities.GetActivity;
using Home.WebUI.DataAccess.Activities.Models;
using Home.WebUI.DataAccess.ActivityContents.CreateActivityContent;
using Home.WebUI.DataAccess.ActivityContents.UpdateActivityContent;
using Home.WebUI.DataAccess.ActivityRegions.CreateActivityRegion;
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

public partial class ActivityDetailPage : IDisposable
{

    #region Fields

    /// <summary>
    /// Fixed by RegionSE in the domain — a card has these three groups and no others. Letting the
    /// family name their own would need a schema change, not a button.
    /// </summary>
    private static readonly string[] RegionKinds = ["Description", "AcceptanceCriteria", "Notes"];

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;

    private GetActivityWebAppResponse? m_Activity;
    private List<ActivityStateDto> m_States = [];
    private List<TagDto> m_Tags = [];
    private List<UserSummaryDto> m_Users = [];

    private bool m_Saving;
    private bool m_ShowEdit;

    // Field
    private bool m_ShowField;
    private string m_FieldRegionKind = string.Empty;
    private long? m_EditingContentID;
    private string m_FieldContent = string.Empty;

    #endregion Fields

    #region Properties

    [Parameter] public long ActivityID { get; set; }
    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(this.LoadStatesAsync(), this.LoadTagsAsync(), this.LoadUsersAsync());

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);
    }

    protected override async Task OnParametersSetAsync()
        => await this.LoadActivityAsync();

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
            await Task.WhenAll(this.LoadActivityAsync(), this.LoadStatesAsync(), this.LoadTagsAsync(), this.LoadUsersAsync());
            this.StateHasChanged();
        });
    }

    // Loading

    private async Task LoadActivityAsync()
        => this.m_Activity = await this.ApiAccess.SendRequestAsync<object, GetActivityWebAppResponse>(
            null!, ApiProvider.GetActivity(this.ActivityID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

    private async Task LoadStatesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetActivityStatesWebAppResponse>(
            null!, ApiProvider.GetActivityStates(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_States = [.. _Result.States.OrderBy(s => s.Sequence)];
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
        await this.LoadActivityAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Activities, this.m_CancellationTokenHandler.Token);
    }

    // The card

    /// <summary>
    /// The shared edit modal works in summaries, which is also what the board hands it — so the
    /// detail response is narrowed to one rather than the modal learning a second shape.
    /// </summary>
    private ActivitySummaryDto? GetSummary()
        => this.m_Activity == null
            ? null
            : new ActivitySummaryDto()
            {
                ActivityID = this.m_Activity.ActivityID,
                AssignedTo = this.m_Activity.AssignedTo,
                AssignedToUserID = this.m_Activity.AssignedToUserID,
                CompletedDateUTC = this.m_Activity.CompletedDateUTC,
                DueDateUTC = this.m_Activity.DueDateUTC,
                DueTime = this.m_Activity.DueTime,
                State = this.m_Activity.State,
                StateID = this.m_Activity.StateID,
                Status = this.m_Activity.Status,
                StatusID = this.m_Activity.StatusID,
                Tags = this.m_Activity.Tags,
                Title = this.m_Activity.Title
            };

    private ActivityRegionDto? GetRegion(string regionKind)
        => this.m_Activity?.Regions.FirstOrDefault(r => r.Region == regionKind);

    private void GoBackToBoard()
        => this.NavigationManager.NavigateTo("/activities");

    // Fields

    private void OpenAddFieldModal(string regionKind)
    {
        this.m_FieldRegionKind = regionKind;
        this.m_EditingContentID = null;
        this.m_FieldContent = string.Empty;
        this.m_ShowField = true;
    }

    private void OpenEditFieldModal(string regionKind, ActivityContentDto field)
    {
        this.m_FieldRegionKind = regionKind;
        this.m_EditingContentID = field.ActivityContentID;
        this.m_FieldContent = field.Content;
        this.m_ShowField = true;
    }

    private async Task SaveFieldAsync()
    {
        if (this.m_Saving || string.IsNullOrWhiteSpace(this.m_FieldContent)) return;
        this.m_Saving = true;

        var _Saved = this.m_EditingContentID.HasValue
            ? await this.UpdateFieldAsync(this.m_EditingContentID.Value)
            : await this.CreateFieldAsync();

        this.m_Saving = false;

        if (!_Saved) return;

        this.m_ShowField = false;
        await this.ReloadAndPublishAsync();
    }

    private async Task<bool> UpdateFieldAsync(long activityContentID)
    {
        var _Request = new UpdateActivityContentWebAppRequest()
        {
            Content = new PropertyChangeTracker<string>(this.m_FieldContent)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateActivityContentWebAppRequest, bool>(
            _Request, ApiProvider.UpdateActivityContent(activityContentID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        return _Result == true;
    }

    /// <summary>
    /// A group is only created the first time something is written into it, so an untouched card
    /// stays empty rather than carrying three blank headings.
    /// </summary>
    private async Task<bool> CreateFieldAsync()
    {
        var _RegionID = this.GetRegion(this.m_FieldRegionKind)?.ActivityRegionID;

        if (_RegionID == null)
        {
            var _Request = new CreateActivityRegionWebAppRequest()
            {
                ActivityID = this.ActivityID,
                Region = this.m_FieldRegionKind
            };

            var _Region = await this.ApiAccess.SendRequestAsync<CreateActivityRegionWebAppRequest, CreateActivityRegionWebAppResponse>(
                _Request, ApiProvider.CreateActivityRegion(),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);

            if (_Region == null)
                return false;

            _RegionID = _Region.ActivityRegionID;
        }

        var _ContentRequest = new CreateActivityContentWebAppRequest()
        {
            ActivityRegionID = _RegionID.Value,
            Content = this.m_FieldContent
        };

        var _Content = await this.ApiAccess.SendRequestAsync<CreateActivityContentWebAppRequest, CreateActivityContentWebAppResponse>(
            _ContentRequest, ApiProvider.CreateActivityContent(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        return _Content != null;
    }

    private async Task DeleteFieldAsync(long activityContentID)
    {
        if (this.m_Saving) return;
        this.m_Saving = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteActivityContent(activityContentID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Saving = false;

        if (_Result == true)
            await this.ReloadAndPublishAsync();
    }

    #endregion Methods

}
