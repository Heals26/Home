using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.UpdateUser;

public class UpdateUserBusinessRuleEvaluator : IBusinessRuleEvaluator<UpdateUserInputPort, IUpdateUserOutputPort>
{

    #region Methods

    async Task<ContinuationBehaviour> IBusinessRuleEvaluator<UpdateUserInputPort, IUpdateUserOutputPort>.EvaluateAsync(
        UpdateUserInputPort inputPort,
        IUpdateUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        // Only a changed, non-blank email can clash, and only with somebody else: until 8 Sep 2026
        // this compared against every member including the one being edited, so saving a member
        // with their own email counted as a conflict.
        if (!inputPort.Email.HasBeenSet || string.IsNullOrWhiteSpace(inputPort.Email.Value))
            return ContinuationBehaviour.Continue;

        var _Persistence = serviceFactory.GetService<IPersistenceContext>();
        var _Email = inputPort.Email.Value.Trim().ToLower();

        if (_Persistence.GetEntities<User>().Any(u => u.UserID != inputPort.UserID && u.Email != null && u.Email.ToLower() == _Email))
            return await outputPort.PresentUserConflictAsync(inputPort.Email.Value, cancellationToken);

        return ContinuationBehaviour.Continue;
    }

    #endregion Methods

}
