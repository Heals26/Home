using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;
using Home.Application.UseCases.Users.CreateUser;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.InterfaceAdapters.Users;

public class CreateUserPresenter : OutputPortPresenter, ICreateUserOutputPort
{
    Task PresentAuthenticationFailureAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    Task<ContinuationBehaviour> PresentInputPortValidationFailureAsync(HomeInputPortValidationFailure validationFailure, CancellationToken cancellationToken) => throw new NotImplementedException();
    Task<ContinuationBehaviour> PresentUserConflictAsync(string email, CancellationToken cancellationToken) => throw new NotImplementedException();
    Task PresentUserCreatedAsync(long userID, CancellationToken cancellationToken) => throw new NotImplementedException();
}
