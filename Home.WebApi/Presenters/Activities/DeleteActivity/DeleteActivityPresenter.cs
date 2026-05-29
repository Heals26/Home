using AutoMapper;
using Home.Application.UseCases.Activities.DeleteActivity;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Activities.DeleteActivity;

public class DeleteActivityPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteActivityOutputPort
{

    #region Methods

    Task IDeleteActivityOutputPort.PresentActivityDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
