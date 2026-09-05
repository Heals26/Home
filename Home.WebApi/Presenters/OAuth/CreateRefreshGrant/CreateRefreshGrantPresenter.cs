using AutoMapper;
using Home.Application.Infrastructure.Values;
using Home.Application.UseCases.OAuth.CreateRefreshGrant;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.OAuth.CreateRefreshGrant;
using Home.WebApi.UseCases.OAuth.Models;

namespace Home.WebApi.Presenters.OAuth.CreateRefreshGrant;

public class CreateRefreshGrantPresenter(IMapper mapper) : OutputPortPresenter(mapper), ICreateRefreshGrantOutputPort
{

    #region Methods

    Task ICreateRefreshGrantOutputPort.PresentAuthorisationGrantedAsync(UserAuthentication data, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<CreateRefreshGrantApiResponse>(data), cancellationToken);

    Task ICreateRefreshGrantOutputPort.PresentNotAuthorisedAsync(OAuthValues error, CancellationToken cancellationToken)
        => this.UnauthorisedWithErrorAsync(new OAuthErrorApiResponse() { Error = error.Name }, cancellationToken);

    #endregion Methods

}
