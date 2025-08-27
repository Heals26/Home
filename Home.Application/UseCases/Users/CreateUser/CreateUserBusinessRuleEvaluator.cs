using CleanArchitecture.Mediator;
using Home.Application.Services.Database;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.CreateUser;

public class CreateUserBusinessRuleEvaluator : IBusinessRuleEvaluator<CreateUserInputPort, ICreateUserOutputPort>
{

    #region <Methods>

    async Task<ContinuationBehaviour> IBusinessRuleEvaluator<CreateUserInputPort, ICreateUserOutputPort>.EvaluateAsync(
        CreateUserInputPort inputPort,
        ICreateUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _Continuation = ContinuationBehaviour.Continue;

        var _PersitenceContext = serviceFactory.GetService<IPersistenceContext>();

        if (_PersitenceContext.GetEntities<User>()
            .Where(u => u.Email.Equals(inputPort.Email, StringComparison.CurrentCultureIgnoreCase))
            .Any())
            _Continuation = ContinuationBehaviour.Return;

        return _Continuation;
    }

    #endregion <Methods>

}

