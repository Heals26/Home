using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.CreateUser;

internal class CreateUserBusinessRuleEvaluator : IBusinessRuleEvaluator<CreateUserInputPort, ICreateUserOutputPort>
{

    #region Methods

    async Task<ContinuationBehaviour> IBusinessRuleEvaluator<CreateUserInputPort, ICreateUserOutputPort>.EvaluateAsync(
        CreateUserInputPort inputPort,
        ICreateUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        // A member without a login has no email to clash with.
        if (string.IsNullOrWhiteSpace(inputPort.Email))
            return ContinuationBehaviour.Continue;

        var _Persistence = serviceFactory.GetService<IPersistenceContext>();
        var _Email = inputPort.Email.Trim().ToLower();

        if (_Persistence.GetEntities<User>().Any(u => u.Email != null && u.Email.ToLower() == _Email))
            return await outputPort.PresentUserConflictAsync(inputPort.Email, cancellationToken);

        return ContinuationBehaviour.Continue;
    }

    #endregion Methods

}
