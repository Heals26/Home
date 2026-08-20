using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightScenes.ApplyLightScene;

internal class ApplyLightSceneInteractor
    : IInteractor<ApplyLightSceneInputPort, IApplyLightSceneOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        ApplyLightSceneInputPort inputPort,
        IApplyLightSceneOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _SceneLogic = serviceFactory.GetService<ILightSceneLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Scene = _PersistenceContext.GetEntities<LightScene>()
            .Where(s => s.LightSceneID == inputPort.LightSceneID
                && s.Household.HouseholdID == _Household.HouseholdID)
            .Select(s => new { Scene = s, States = s.States.Select(st => new { State = st, st.Light }) })
            .SingleOrDefault()
            ?.Scene;

        if (_Scene == null)
        {
            await outputPort.PresentLightSceneNotFoundAsync(inputPort.LightSceneID, cancellationToken);
            return;
        }

        // Snapshotted before the apply mutates the cached light rows, so "Previous look" is what
        // the room actually looked like a moment ago — including when the scene being applied is
        // the previous look itself, which is what makes tapping it twice toggle back and forth.
        var _PreviousStates = this.SnapshotHouseholdLights(_Household, _PersistenceContext);

        var _Result = await _SceneLogic.ApplyAsync(_Scene, cancellationToken);

        if (_Result == LightCommandResult.Unavailable)
        {
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
            return;
        }

        this.SavePreviousLook(_Household, _PreviousStates, _PersistenceContext);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightSceneAppliedAsync(cancellationToken);
    }

    /// <summary>
    /// The whole household rather than just the lights the scene touches, because "how it looked
    /// before" means the room, not the subset that changed.
    /// </summary>
    private List<LightSceneState> SnapshotHouseholdLights(Household household, IPersistenceContext persistenceContext)
        => [.. persistenceContext.GetEntities<Light>()
            .Where(l => l.Group.Location.Household.HouseholdID == household.HouseholdID)
            .ToList()
            .Select(l => new LightSceneState()
            {
                Light = l,
                Brightness = l.Brightness,
                Hue = l.Hue,
                IsOn = l.IsOn,
                Kelvin = l.Kelvin,
                Saturation = l.Saturation
            })];

    private void SavePreviousLook(Household household, List<LightSceneState> states, IPersistenceContext persistenceContext)
    {
        var _PreviousLook = persistenceContext.GetEntities<LightScene>()
            .Where(s => s.Household.HouseholdID == household.HouseholdID && s.IsPreviousLook)
            .Select(s => new { Scene = s, s.States })
            .SingleOrDefault()
            ?.Scene;

        if (_PreviousLook == null)
        {
            _PreviousLook = new LightScene()
            {
                Household = household,
                IsPreviousLook = true,
                Name = "Previous look",
                Sequence = -1,
                States = []
            };

            persistenceContext.Add(_PreviousLook);
        }
        else
        {
            persistenceContext.RemoveRange(_PreviousLook.States);
            _PreviousLook.States.Clear();
        }

        states.ForEach(s =>
        {
            s.Scene = _PreviousLook;
            _PreviousLook.States.Add(s);
        });
    }

    #endregion Methods

}
