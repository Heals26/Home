using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Users.CreateUser;

public interface ICreateUserOutputPort : IAuthenticationFailureOutputPort,
    IAuthorisationPolicyFailureOutputPort<HomeAuthorisationFailure>,
    IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task<ContinuationBehaviour> PresentUserConflictAsync(string email, CancellationToken cancellationToken);
    Task PresentUserCreatedAsync(long userID, CancellationToken cancellationToken);

    #endregion Methods

}

