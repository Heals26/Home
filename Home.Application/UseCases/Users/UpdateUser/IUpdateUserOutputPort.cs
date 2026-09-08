using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Users.UpdateUser;

public interface IUpdateUserOutputPort : IAuthenticationFailureOutputPort,
    IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    /// <summary>
    /// A password was sent for a member who has no email to sign in with.
    /// </summary>
    Task PresentLoginNeedsEmailAsync(CancellationToken cancellationToken);

    Task<ContinuationBehaviour> PresentUserConflictAsync(string email, CancellationToken cancellationToken);
    Task PresentUserNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
