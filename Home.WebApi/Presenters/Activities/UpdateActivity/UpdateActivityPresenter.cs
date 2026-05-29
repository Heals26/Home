using AutoMapper;
using Home.Application.UseCases.Activities.UpdateActivity;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Activities.UpdateActivity;

public class UpdateActivityPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateActivityOutputPort
{

    #region Methods

    Task IUpdateActivityOutputPort.PresentActivityNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateActivityOutputPort.PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity {activityID} Not Found", cancellationToken);

    #endregion Methods

}
