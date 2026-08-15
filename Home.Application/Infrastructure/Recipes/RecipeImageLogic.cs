namespace Home.Application.Infrastructure.Recipes;

public static class RecipeImageLogic
{

    #region Methods

    /// <summary>
    /// Only absolute http or https addresses are ever stored or rendered — anything else is a
    /// scheme the tablet has no business following.
    /// </summary>
    public static bool IsAWebAddress(string? url)
        => !string.IsNullOrWhiteSpace(url)
            && Uri.TryCreate(url, UriKind.Absolute, out var _Uri)
            && (_Uri.Scheme == Uri.UriSchemeHttp || _Uri.Scheme == Uri.UriSchemeHttps);

    #endregion Methods

}
