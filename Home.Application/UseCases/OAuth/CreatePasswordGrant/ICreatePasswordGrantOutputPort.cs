using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Values;
using Home.Domain.Entities;

namespace Home.Application.UseCases.OAuth.CreatePasswordGrant;

public interface ICreatePasswordGrantOutputPort : IAuthenticationFailureOutputPort
{

    #region Methods

    Task PresentNotAuthorisedAsync(OAuthValues error, CancellationToken cancellationToken);
    Task PresentAuthorisationGrantedAsync(AuthenticationMetadata data, CancellationToken cancellationToken);

    #endregion Methods

}
