using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.Security;
using Home.Application.Services.Undo;

namespace Home.Application.UseCases.Undo.UndoAction;

/// <summary>
/// The Undo on the bar. Only the device that made the request knows its token, so an undo is always
/// the undo of something that device did.
/// </summary>
internal class UndoActionInteractor : IInteractor<UndoActionInputPort, IUndoActionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UndoActionInputPort inputPort,
        IUndoActionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _UndoStore = serviceFactory.GetService<IUndoStore>();
        var _TimeProvider = serviceFactory.GetService<TimeProvider>();

        var _Household = _AuthorisationService.GetHousehold();
        var _NotBeforeUTC = _TimeProvider.GetUtcNow().UtcDateTime - UndoValues.HonouredFor;

        switch (await _UndoStore.UndoAsync(_Household.HouseholdID, inputPort.Token, _NotBeforeUTC, cancellationToken))
        {
            case UndoOutcome.Undone:
                await outputPort.PresentActionUndoneNoContentAsync(cancellationToken);
                break;
            case UndoOutcome.CannotUndo:
                await outputPort.PresentUndoRefusedAsync(cancellationToken);
                break;
            case UndoOutcome.Expired:
                await outputPort.PresentUndoExpiredAsync(cancellationToken);
                break;
            default:
                await outputPort.PresentUndoNotFoundAsync(inputPort.Token, cancellationToken);
                break;
        }
    }

    #endregion Methods

}
