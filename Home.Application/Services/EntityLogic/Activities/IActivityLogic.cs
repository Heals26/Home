using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Activities;

public interface IActivityLogic
{

    #region Methods

    User CreateActivity();
    User UpdateActivity();
    User DeleteActivity(long activityID);
    User GetActivity(long activityID);
    User GetActivities();

    #endregion Methods

}
