namespace Home.WebApi.UseCases.LightScenes.CaptureLightScene;

/// <summary>
/// Saves the current look. Leave LightGroupID null to capture every light in the household.
/// </summary>
public record CaptureLightSceneApiRequest(string Name, long? LightGroupID);
