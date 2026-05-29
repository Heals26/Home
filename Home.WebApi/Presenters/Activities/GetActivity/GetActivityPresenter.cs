using AutoMapper;
using Home.Application.UseCases.Activities.GetActivity;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Activities.GetActivity;
using Home.WebApi.UseCases.Activities.Models;

namespace Home.WebApi.Presenters.Activities.GetActivity;

public class GetActivityPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetActivityOutputPort
{

    #region Methods

    Task IGetActivityOutputPort.PresentActivityAsync(Activity activity, CancellationToken cancellationToken)
        => this.OkAsync(new GetActivityApiResponse()
        {
            ActivityID = activity.ActivityID,
            Title = activity.Title,
            DueDateUTC = activity.DueDateUTC,
            CompletedDateUTC = activity.CompletedDateUTC,
            StateID = activity.State?.ActivityStateID,
            State = activity.State?.Name,
            StatusID = activity.Status?.ActivityStatusID,
            Status = activity.Status?.Name,
            AssignedToUserID = activity.User?.UserID,
            AssignedTo = activity.User?.UserName,
            Regions = [.. activity.Regions.OrderBy(r => r.Sequence).Select(r => new ActivityRegionDto()
            {
                ActivityRegionID = r.ActivityRegionID,
                Region = r.Region?.Name,
                Sequence = r.Sequence,
                Fields = [.. r.Fields.OrderBy(f => f.Sequence).Select(f => new ActivityContentDto()
                {
                    ActivityContentID = f.ActivityContentID,
                    Content = f.Content,
                    Sequence = f.Sequence
                })]
            })]
        }, cancellationToken);

    Task IGetActivityOutputPort.PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity {activityID} Not Found", cancellationToken);

    #endregion Methods

}
