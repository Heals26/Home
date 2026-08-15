using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.UseCases.Tags.UpdateTag;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Tags.UpdateTag;

public class UpdateTagPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateTagOutputPort
{

    #region Methods

    Task<ContinuationBehaviour> IUpdateTagOutputPort.PresentTagNameTakenAsync(string name, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IUpdateTagOutputPort.PresentTagNotFoundAsync(long tagID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Tag {tagID} Not Found", cancellationToken);

    Task IUpdateTagOutputPort.PresentTagUpdatedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
