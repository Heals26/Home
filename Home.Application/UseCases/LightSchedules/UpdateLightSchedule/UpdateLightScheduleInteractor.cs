using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightSchedules.UpdateLightSchedule;

internal class UpdateLightScheduleInteractor
    : IInteractor<UpdateLightScheduleInputPort, IUpdateLightScheduleOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateLightScheduleInputPort inputPort,
        IUpdateLightScheduleOutputPort outputPort,
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

        if (inputPort.Name.HasBeenSet)
            _Schedule.Name = inputPort.Name.Value.Trim();

        if (inputPort.IsEnabled.HasBeenSet)
            _Schedule.IsEnabled = inputPort.IsEnabled.Value;

        // Moving the time earlier in the day could otherwise make it fire again immediately, so
        // clear the last-run marker and let the normal due check decide.
        if (inputPort.TimeOfDay.HasBeenSet && inputPort.TimeOfDay.Value != _Schedule.TimeOfDay)
        {
            _Schedule.TimeOfDay = inputPort.TimeOfDay.Value;
            _Schedule.LastRunUTC = serviceFactory.GetService<TimeProvider>().GetUtcNow().UtcDateTime;
        }

        if (inputPort.DaysOfWeek.HasBeenSet)
            _Schedule.DaysOfWeek = inputPort.DaysOfWeek.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightScheduleUpdatedAsync(cancellationToken);
    }

    #endregion Methods

}
