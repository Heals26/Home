using Home.Application.Services.Lights;
using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Lights;

/// <summary>
/// Applying a scene is shared between the API (a user tapping it) and the schedule runner (a timer
/// firing it), so the batching and cache-writing lives here rather than in either caller.
/// </summary>
public interface ILightSceneLogic
{

    #region Methods

    Task<LightCommandResult> ApplyAsync(LightScene scene, CancellationToken cancellationToken);

    #endregion Methods

}
