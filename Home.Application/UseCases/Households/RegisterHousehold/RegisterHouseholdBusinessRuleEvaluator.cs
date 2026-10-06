using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Households.RegisterHousehold;

/// <summary>
/// Signing in finds a member by email across every household, so an email can belong to one member
/// only. The same rule the member slices apply, here for the first member of a new household.
/// </summary>
internal class RegisterHouseholdBusinessRuleEvaluator : IBusinessRuleEvaluator<RegisterHouseholdInputPort, IRegisterHouseholdOutputPort>
{

    #region Methods

    async Task<ContinuationBehaviour> IBusinessRuleEvaluator<RegisterHouseholdInputPort, IRegisterHouseholdOutputPort>.EvaluateAsync(
        RegisterHouseholdInputPort inputPort,
        IRegisterHouseholdOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _Persistence = serviceFactory.GetService<IPersistenceContext>();
        var _Email = inputPort.Email.Trim().ToLower();

        if (_Persistence.GetEntities<User>().Any(u => u.Email != null && u.Email.ToLower() == _Email))
            return await outputPort.PresentUserConflictAsync(inputPort.Email, cancellationToken);

        return ContinuationBehaviour.Continue;
    }

    #endregion Methods

}
