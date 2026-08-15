using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityStates.GetActivityStates;

internal class GetActivityStatesInteractor : IInteractor<GetActivityStatesInputPort, IGetActivityStatesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityStatesInputPort input,
        IGetActivityStatesOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ActivityStates = _PersistenceContext.GetEntities<ActivityState>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(s => s.Sequence)
            .ThenBy(s => s.ActivityStateID)
            .ToList();

        await output.PresentActivityStatesAsync(_ActivityStates, cancellationToken);
    }

    #endregion Methods

}
