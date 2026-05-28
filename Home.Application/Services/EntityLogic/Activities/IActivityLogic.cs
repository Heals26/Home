using Home.Application.UseCases.ActivityContents.CreateActivityContent;
using Home.Application.UseCases.ActivityContents.UpdateActivityContent;
using Home.Application.UseCases.ActivityRegions.CreateActivityRegion;
using Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;
using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Activities;

public interface IActivityLogic
{

    #region Methods

    ActivityRegion AddRegion(CreateActivityRegionInputPort inputPort);
    ActivityContent AddContent(CreateActivityContentInputPort inputPort);
    bool DoesActivityRegionExist(long activityRegionID);
    bool DoesActivityContentExist(long activityContentID);
    void UpdateRegion(UpdateActivityRegionInputPort inputPort);
    void UpdateContent(UpdateActivityContentInputPort inputPort);

    #endregion Methods

}
