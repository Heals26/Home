using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightSchedules.DeleteLightSchedule;

internal class DeleteLightScheduleInteractor
    : IInteractor<DeleteLightScheduleInputPort, IDeleteLightScheduleOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteLightScheduleInputPort inputPort,
        IDeleteLightScheduleOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Schedule = _PersistenceContext.GetEntities<LightSchedule>()
            .Where(s => s.LightScheduleID == inputPort.LightScheduleID
                && s.Scene.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Schedule == null)
        {
            await outputPort.PresentLightScheduleNotFoundAsync(inputPort.LightScheduleID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_Schedule);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightScheduleDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
