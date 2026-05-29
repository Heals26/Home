using AutoMapper;
using Home.Application.UseCases.ActivityContents.UpdateActivityContent;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ActivityContents.UpdateActivityContent;

public class UpdateActivityContentPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateActivityContentOutputPort
{

    #region Methods

    Task IUpdateActivityContentOutputPort.PresentActivityContentNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateActivityContentOutputPort.PresentActivityContentNotFoundAsync(long activityContentID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity Content {activityContentID} Not Found", cancellationToken);

    #endregion Methods

}
