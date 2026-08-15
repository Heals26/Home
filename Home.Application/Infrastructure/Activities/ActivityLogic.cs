using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.ActivityContents.CreateActivityContent;
using Home.Application.UseCases.ActivityContents.UpdateActivityContent;
using Home.Application.UseCases.ActivityRegions.CreateActivityRegion;
using Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.Activities;

public class ActivityLogic(IPersistenceContext persistenceContext, TimeProvider timeProvider) : IActivityLogic
{

    #region Methods

    ActivityRegion IActivityLogic.AddRegion(CreateActivityRegionInputPort inputPort)
    {
        var _Activity = persistenceContext.GetEntities<Activity>()
            .Where(a => a.ActivityID == inputPort.ActivityID)
            .Select(a => new
            {
                Activity = a,
                a.Regions
            })
            .Single()
            .Activity;

        return new ActivityRegion()
        {
            Region = (RegionSE)inputPort.Region,
            Sequence = (_Activity.Regions?.Count + 1) ?? 1,
            Fields = []
        };
    }

    ActivityContent IActivityLogic.AddContent(CreateActivityContentInputPort inputPort)
    {
        var _Region = persistenceContext.GetEntities<ActivityRegion>()
            .Where(r => r.ActivityRegionID == inputPort.ActivityRegionID)
            .Select(r => new
            {
                Region = r,
                r.Fields
            })
            .Single()
            .Region;

        return new ActivityContent()
        {
            Content = inputPort.Content,
            Sequence = (_Region.Fields?.Count + 1) ?? 1
        };
    }

    void IActivityLogic.ApplyStateChange(Activity activity, ActivityState? state)
    {
        activity.State = state;

        // A date already on the card wins, so an explicitly entered completion date is not
        // overwritten by the drag that put the card in the column.
        if (state?.IsComplete == true)
            activity.CompletedDateUTC ??= timeProvider.GetUtcNow().UtcDateTime;
        else
            activity.CompletedDateUTC = null;
    }

    bool IActivityLogic.DoesActivityRegionExist(long activityRegionID)
        => persistenceContext.DoesEntityExist<ActivityRegion>(activityRegionID);

    bool IActivityLogic.DoesActivityContentExist(long activityContentID)
        => persistenceContext.DoesEntityExist<ActivityContent>(activityContentID);

    void IActivityLogic.UpdateRegion(UpdateActivityRegionInputPort inputPort)
    {
        var _ActivityRegion = persistenceContext.GetEntities<ActivityRegion>()
            .Where(r => r.ActivityRegionID == inputPort.ActivityRegionID)
            .Select(r => new
            {
                Region = r
            })
            .Single()
            .Region;

        if (inputPort.Region.HasBeenSet)
            _ActivityRegion.Region = (RegionSE)inputPort.Region.Value;

        if (inputPort.Sequence.HasBeenSet)
            _ActivityRegion.Sequence = inputPort.Sequence.Value;
    }

    void IActivityLogic.UpdateContent(UpdateActivityContentInputPort inputPort)
    {
        var _ActivityContent = persistenceContext.GetEntities<ActivityContent>()
            .Where(c => c.ActivityContentID == inputPort.ActivityContentID)
            .Select(c => new
            {
                Content = c
            })
            .Single()
            .Content;

        if (inputPort.Content.HasBeenSet)
            _ActivityContent.Content = inputPort.Content.Value;

        if (inputPort.Sequence.HasBeenSet)
            _ActivityContent.Sequence = inputPort.Sequence.Value;
    }

    #endregion Methods

}
