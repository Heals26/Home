namespace Home.Application.Infrastructure.Recipes;

/// <summary>
/// The one place that decides what counts as an acceptable recipe picture, linked or uploaded.
/// </summary>
public static class RecipeImageLogic
{

    #region Fields

    /// <summary>
    /// Big enough for any phone photo worth keeping, small enough that a recipe row can't eat
    /// the database.
    /// </summary>
    public const int MaximumContentBytes = 5 * 1024 * 1024;

    #endregion Fields

    #region Methods

    /// <summary>
    /// Reads the format off the bytes themselves — the formats every browser draws, or null.
    /// The declared content type is never trusted: a renamed file lies, the bytes don't.
    /// </summary>
    public static string? DetectContentType(byte[] content)
    {
        if (content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
            return "image/jpeg";

        if (content.Length >= 4 && content[0] == 0x89 && content[1] == (byte)'P' && content[2] == (byte)'N' && content[3] == (byte)'G')
            return "image/png";

        if (content.Length >= 4 && content[0] == (byte)'G' && content[1] == (byte)'I' && content[2] == (byte)'F' && content[3] == (byte)'8')
            return "image/gif";

        if (content.Length >= 12
            && content[0] == (byte)'R' && content[1] == (byte)'I' && content[2] == (byte)'F' && content[3] == (byte)'F'
            && content[8] == (byte)'W' && content[9] == (byte)'E' && content[10] == (byte)'B' && content[11] == (byte)'P')
            return "image/webp";

        return null;
    }

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
