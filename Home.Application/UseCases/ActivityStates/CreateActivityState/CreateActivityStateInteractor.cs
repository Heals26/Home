using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityStates.CreateActivityState;

internal class CreateActivityStateInteractor
    : IInteractor<CreateActivityStateInputPort, ICreateActivityStateOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateActivityStateInputPort inputPort,
        ICreateActivityStateOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // A new column joins the right-hand end of the board; reordering is a separate update.
        var _Sequence = (_PersistenceContext.GetEntities<ActivityState>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .Max(s => (int?)s.Sequence) + 1) ?? 0;

        var _ActivityState = new ActivityState()
        {
            Activities = [],
            Household = _Household,
            IsComplete = inputPort.IsComplete,
            Name = inputPort.Name.Trim(),
            Sequence = _Sequence
        };

        _PersistenceContext.Add(_ActivityState);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityStateCreatedAsync(_ActivityState.ActivityStateID, cancellationToken);
    }

    #endregion Methods

}
