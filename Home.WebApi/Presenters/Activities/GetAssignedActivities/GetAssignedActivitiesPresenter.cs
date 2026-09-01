using AutoMapper;
using Home.Application.UseCases.Activities.GetAssignedActivities;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Activities.GetActivities;
using Home.WebApi.UseCases.Activities.Models;
using Home.WebApi.UseCases.Tags.Models;

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
                DueTime = a.DueTime,
                CompletedDateUTC = a.CompletedDateUTC,
                StateID = a.State?.ActivityStateID,
                State = a.State?.Name,
                AssignedToUserID = a.User?.UserID,
                AssignedTo = a.User?.UserName,
                Tags = [.. a.Tags.Select(t => t.Tag).OrderBy(t => t.Name).Select(t => new TagDto()
                {
                    Colour = t.Colour,
                    Name = t.Name,
                    TagID = t.TagID
                })]
            })]
        }, cancellationToken);

    #endregion Methods

}
