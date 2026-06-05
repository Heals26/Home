using AutoMapper;
using Home.Application.UseCases.ActivityStates.GetActivityStates;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ActivityStates.GetActivityStates;
using Home.WebApi.UseCases.ActivityStates.Models;

namespace Home.WebApi.Presenters.ActivityStates.GetActivityStates;

public class GetActivityStatesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetActivityStatesOutputPort
{

    #region Methods

    Task IGetActivityStatesOutputPort.PresentActivityStatesAsync(IEnumerable<ActivityState> activityStates, CancellationToken cancellationToken)
        => this.OkAsync(new GetActivityStatesApiResponse()
        {
            States = [.. activityStates.Select(s => new ActivityStateDto()
            {
                ActivityStateID = s.ActivityStateID,
                Name = s.Name
            })]
        }, cancellationToken);

    #endregion Methods

}
