using AutoMapper;
using Home.Application.Infrastructure.Values;
using Home.Application.UseCases.OAuth.CreatePasswordGrant;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.OAuth.CreateRefreshGrant;

namespace Home.WebApi.Presenters.OAuth;

public class CreatePasswordGrantPresenter(IMapper mapper) : OutputPortPresenter(mapper), ICreatePasswordGrantOutputPort
{

    #region Methods

    Task ICreatePasswordGrantOutputPort.PresentAuthorisationGrantedAsync(AuthenticationMetadata data, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<CreateRefreshGrantApiResponse>(data), cancellationToken);

    Task ICreatePasswordGrantOutputPort.PresentNotAuthorisedAsync(OAuthValues error, CancellationToken cancellationToken)
        => this.UnauthorisedAsync(new(error.Name), cancellationToken);

    #endregion Methods

}
