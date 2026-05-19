using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.Users.CreateUser;
using Home.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Application.UseCases.Users.UpdateUser;

public class UpdateUserBusinessRuleEvaluator : IBusinessRuleEvaluator<UpdateUserInputPort, IUpdateUserOutputPort>
{

    #region Methods

    Task<ContinuationBehaviour> IBusinessRuleEvaluator<UpdateUserInputPort, IUpdateUserOutputPort>.EvaluateAsync(
        UpdateUserInputPort inputPort,
        IUpdateUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _Continuation = ContinuationBehaviour.Continue;

        var _Persistence = serviceFactory.GetService<IPersistenceContext>();

        if (_Persistence.GetEntities<User>()
            .Where(u => u.Email.Equals(inputPort.Email, StringComparison.CurrentCultureIgnoreCase))
            .Any())
            _Continuation = ContinuationBehaviour.Return;

        return Task.FromResult(_Continuation);
    }

    #endregion Methods

}
