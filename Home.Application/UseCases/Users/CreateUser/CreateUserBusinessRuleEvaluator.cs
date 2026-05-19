using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.CreateUser;

internal class CreateUserBusinessRuleEvaluator : IBusinessRuleEvaluator<CreateUserInputPort, ICreateUserOutputPort>
{

    #region Methods

    Task<ContinuationBehaviour> IBusinessRuleEvaluator<CreateUserInputPort, ICreateUserOutputPort>.EvaluateAsync(
        CreateUserInputPort inputPort,
        ICreateUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _Continuation = ContinuationBehaviour.Continue;

        var _Persistence = serviceFactory.GetService<IPersistenceContext>();

        if (_Persistence.GetEntities<User>()
            .Any(u => u.Email.ToLower() == inputPort.Email.ToLower()))
            _Continuation = ContinuationBehaviour.Return;

        return Task.FromResult(_Continuation);
    }

    #endregion Methods

}

