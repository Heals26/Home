using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;
using Home.Application.UseCases.Users.CreateUser;

namespace Home.WebApi.InterfaceAdapters.Users;

public class CreateUserPresenter : ICreateUserOutputPort
{
    public Task PresentAuthenticationFailureAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ContinuationBehaviour> PresentAuthorisationPolicyFailureAsync(HomeAuthorisationFailure policyFailure, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ContinuationBehaviour> PresentInputPortValidationFailureAsync(HomeInputPortValidationFailure validationFailure, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ContinuationBehaviour> PresentUserConflictAsync(string email, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task PresentUserCreatedAsync(long userID, CancellationToken cancellationToken) => throw new NotImplementedException();
}
