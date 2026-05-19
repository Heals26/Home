using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Values;
using Home.Domain.Entities;

namespace Home.Application.UseCases.OAuth.CreateRefreshGrant;

public interface ICreateRefreshGrantOutputPort : IAuthenticationFailureOutputPort
{

    #region Methods

    Task PresentNotAuthorisedAsync(OAuthValues error, CancellationToken cancellationToken);
    Task PresentAuthorisationGrantedAsync(UserAuthentication data, CancellationToken cancellationToken);

    #endregion Methods

}
