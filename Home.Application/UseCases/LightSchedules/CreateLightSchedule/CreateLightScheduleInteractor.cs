using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightSchedules.CreateLightSchedule;

internal class CreateLightScheduleInteractor
    : IInteractor<CreateLightScheduleInputPort, ICreateLightScheduleOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateLightScheduleInputPort inputPort,
        ICreateLightScheduleOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Scene = _PersistenceContext.GetEntities<LightScene>()
            .Where(s => s.LightSceneID == inputPort.LightSceneID
                && s.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Scene == null)
        {
            await outputPort.PresentLightSceneNotFoundAsync(inputPort.LightSceneID, cancellationToken);
            return;
        }

        var _Schedule = new LightSchedule()
        {
            Name = inputPort.Name.Trim(),
            Scene = _Scene,
            Trigger = inputPort.Trigger,
            TimeOfDay = inputPort.TimeOfDay,
            OffsetMinutes = inputPort.OffsetMinutes,
            DaysOfWeek = inputPort.DaysOfWeek,
            IsEnabled = true
        };

        _PersistenceContext.Add(_Schedule);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightScheduleCreatedAsync(_Schedule.LightScheduleID, cancellationToken);
    }

    #endregion Methods

}
