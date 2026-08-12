namespace Home.Application.Services.Lights;

/// <summary>
/// The outcome of a command sent to the lighting provider. Three states rather than a bool,
/// because "that bulb does not exist" and "the provider is down" need different responses.
/// </summary>
public enum LightCommandResult
{

    /// <summary>The provider accepted and applied the change.</summary>
    Applied = 0,

    /// <summary>The provider is reachable but knows no light with that ID.</summary>
    LightNotFound = 1,

    /// <summary>The provider could not be reached, timed out, or rejected our credentials.</summary>
    Unavailable = 2,

}
