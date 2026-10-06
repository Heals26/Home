namespace Home.WebUI.Infrastructure.Features;

/// <summary>
/// One thing the app can do, in the words a person would use for it.
/// </summary>
/// <param name="Name">What it is called, in four words or so.</param>
/// <param name="What">What it does, in one plain sentence.</param>
public record Feature(string Name, string What)
{

    #region Properties

    /// <summary>
    /// The piece of the app shown underneath, for a feature that is easier seen than described.
    /// </summary>
    public FeatureSample Sample { get; init; }

    #endregion Properties

}
