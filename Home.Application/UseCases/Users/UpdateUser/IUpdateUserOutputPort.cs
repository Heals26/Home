using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Users.UpdateUser;

public interface IUpdateUserOutputPort : IAuthenticationFailureOutputPort,
    IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task<ContinuationBehaviour> PresentUserConflictAsync(string email, CancellationToken cancellationToken);
    Task PresentUserNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
