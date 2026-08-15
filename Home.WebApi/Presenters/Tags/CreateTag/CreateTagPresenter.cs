using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.UseCases.Tags.CreateTag;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Tags.CreateTag;

namespace Home.WebApi.Presenters.Tags.CreateTag;

public class CreateTagPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateTagOutputPort
{

    #region Methods

    Task ICreateTagOutputPort.PresentTagCreatedAsync(long tagID, CancellationToken cancellationToken)
        => this.CreatedAsync(tagID, new CreateTagApiResponse() { TagID = tagID }, cancellationToken);

    Task<ContinuationBehaviour> ICreateTagOutputPort.PresentTagNameTakenAsync(string name, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    #endregion Methods

}
