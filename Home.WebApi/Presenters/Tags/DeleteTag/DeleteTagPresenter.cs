using AutoMapper;
using Home.Application.UseCases.Tags.DeleteTag;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Tags.DeleteTag;

public class DeleteTagPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteTagOutputPort
{

    #region Methods

    Task IDeleteTagOutputPort.PresentTagDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteTagOutputPort.PresentTagNotFoundAsync(long tagID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Tag {tagID} Not Found", cancellationToken);

    #endregion Methods

}
