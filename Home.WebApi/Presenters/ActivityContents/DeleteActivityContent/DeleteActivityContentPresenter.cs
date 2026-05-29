using AutoMapper;
using Home.Application.UseCases.ActivityContents.DeleteActivityContent;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ActivityContents.DeleteActivityContent;

public class DeleteActivityContentPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteActivityContentOutputPort
{

    #region Methods

    Task IDeleteActivityContentOutputPort.PresentActivityContentDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteActivityContentOutputPort.PresentActivityContentNotFoundAsync(long activityContentID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity Content {activityContentID} Not Found", cancellationToken);

    #endregion Methods

}
