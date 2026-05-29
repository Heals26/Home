using AutoMapper;
using Home.Application.UseCases.Activities.GetAssignedActivities;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Activities.GetActivities;
using Home.WebApi.UseCases.Activities.Models;

namespace Home.WebApi.Presenters.Activities.GetAssignedActivities;

public class GetAssignedActivitiesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetAssignedActivitiesOutputPort
{

    #region Methods

    Task IGetAssignedActivitiesOutputPort.PresentAssignedActivitiesAsync(IEnumerable<Activity> activities, CancellationToken cancellationToken)
        => this.OkAsync(new GetActivitiesApiResponse()
        {
            Activities = [.. activities.Select(a => new ActivitySummaryDto()
            {
                ActivityID = a.ActivityID,
                Title = a.Title,
                DueDateUTC = a.DueDateUTC,
                CompletedDateUTC = a.CompletedDateUTC,
                StateID = a.State?.ActivityStateID,
                State = a.State?.Name,
                StatusID = a.Status?.ActivityStatusID,
                Status = a.Status?.Name,
                AssignedToUserID = a.User?.UserID,
                AssignedTo = a.User?.UserName
            })]
        }, cancellationToken);

    #endregion Methods

}
