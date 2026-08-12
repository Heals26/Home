using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightScenes.CaptureLightScene;

/// <summary>
/// Snapshots the cached state of the chosen lights into a named scene. Reads Home's own records,
/// so capturing costs no provider calls — sync first if the cache might be stale.
/// </summary>
internal class CaptureLightSceneInteractor
    : IInteractor<CaptureLightSceneInputPort, ICaptureLightSceneOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CaptureLightSceneInputPort inputPort,
        ICaptureLightSceneOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Lights = _PersistenceContext.GetEntities<Light>()
            .Where(l => l.Group.Location.Household.HouseholdID == _Household.HouseholdID
                && (inputPort.LightGroupID == null || l.Group.LightGroupID == inputPort.LightGroupID))
            .ToList();

        if (_Lights.Count == 0)
        {
            await outputPort.PresentNoLightsToCaptureAsync(cancellationToken);
            return;
        }

        var _Sequence = _PersistenceContext.GetEntities<LightScene>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .Select(s => (int?)s.Sequence)
            .Max() ?? -1;

        var _Scene = new LightScene()
        {
            Name = inputPort.Name.Trim(),
            Household = _Household,
            Sequence = _Sequence + 1,
            States = []
        };

        foreach (var _Light in _Lights)
        {
            _Scene.States.Add(new LightSceneState()
            {
                Light = _Light,
                Scene = _Scene,
                Brightness = _Light.Brightness,
                Hue = _Light.Hue,
                IsOn = _Light.IsOn,
                Kelvin = _Light.Kelvin,
                Saturation = _Light.Saturation
            });
        }

        _PersistenceContext.Add(_Scene);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightSceneCapturedAsync(_Scene.LightSceneID, _Lights.Count, cancellationToken);
    }

    #endregion Methods

}
