using CleanArchitecture.Mediator;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Lights.StartLightEffect;

internal class StartLightEffectInteractor
    : IInteractor<StartLightEffectInputPort, IStartLightEffectOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        StartLightEffectInputPort inputPort,
        IStartLightEffectOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _LightService = serviceFactory.GetService<ILightService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Lights = _PersistenceContext.GetEntities<Light>()
            .Where(l => l.Group.Location.Household.HouseholdID == _Household.HouseholdID
                && l.IsConnected
                && (inputPort.LightGroupID == null || l.Group.LightGroupID == inputPort.LightGroupID))
            .ToList();

        if (_Lights.Count == 0 && inputPort.LightGroupID != null)
        {
            var _GroupExists = _PersistenceContext.GetEntities<LightGroup>()
                .Any(g => g.LightGroupID == inputPort.LightGroupID
                    && g.Location.Household.HouseholdID == _Household.HouseholdID);

            if (!_GroupExists)
            {
                await outputPort.PresentLightGroupNotFoundAsync(inputPort.LightGroupID.Value, cancellationToken);
                return;
            }
        }

        if (_Lights.Count == 0)
        {
            await outputPort.PresentEffectStartedAsync(cancellationToken);
            return;
        }

        var _Effect = new LightEffectRequest(
            inputPort.Kind,
            inputPort.Hue,
            inputPort.Saturation,
            inputPort.PeriodSeconds,
            inputPort.Cycles,
            PowerOn: inputPort.Kind != LightEffectKind.Off);

        var _Result = await _LightService.StartEffectAsync(
            [.. _Lights.Select(l => l.ID)], _Effect, cancellationToken);

        // An effect is transient and does not persist, so Home's cached state is still correct
        // once it finishes — nothing to write back.
        if (_Result == LightCommandResult.Unavailable)
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
        else
            await outputPort.PresentEffectStartedAsync(cancellationToken);
    }

    #endregion Methods

}
