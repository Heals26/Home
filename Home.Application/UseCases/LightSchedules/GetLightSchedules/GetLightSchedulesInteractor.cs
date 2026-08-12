using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightSchedules.GetLightSchedules;

internal class GetLightSchedulesInteractor
    : IInteractor<GetLightSchedulesInputPort, IGetLightSchedulesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetLightSchedulesInputPort inputPort,
        IGetLightSchedulesOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Schedules = _PersistenceContext.GetEntities<LightSchedule>()
            .Where(s => s.Scene.Household.HouseholdID == _Household.HouseholdID)
            .Select(s => new { Schedule = s, s.Scene })
            .ToList()
            .Select(s => s.Schedule)
            .OrderBy(s => s.TimeOfDay)
            .ThenBy(s => s.Name)
            .ToList();

        await outputPort.PresentLightSchedulesAsync(_Schedules, cancellationToken);
    }

    #endregion Methods

}
