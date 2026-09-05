using System.Text;

namespace Home.WebUI.Infrastructure.Configuration;

/// <summary>
/// Everything this project needs before it can serve a page, checked once at startup.
/// <para>
/// The OAuth values used to be read at the moment of use with a null-forgiving operator, so a
/// missing one reached the API as an empty credential and came back as a bare 401. The sign-in
/// page then reported it as a wrong password, which is the worst possible answer: it is not the
/// family's mistake and no amount of retyping fixes it. Failing at startup, naming every setting
/// that is wrong at once, is the whole point of this class.
/// </para>
/// </summary>
public static class RequiredConfiguration
{

    #region Constants

    /// <summary>
    /// A base URL already carrying this is the documented mistake worth catching: `ApiProvider`
    /// prefixes every route with `api`, so the calls come out as `.../api/api/Recipes`.
    /// </summary>
    private const string DuplicatedApiSegment = "/api";

    #endregion Constants

    #region Methods

    private static string BuildMessage(IReadOnlyList<RequiredSetting> problems)
    {
        var _Message = new StringBuilder()
            .AppendLine($"Home.WebUI cannot start. {problems.Count} setting{(problems.Count == 1 ? " is" : "s are")} missing or invalid.")
            .AppendLine();

        foreach (var _Problem in problems)
        {
            _ = _Message
                .AppendLine($"  {_Problem.Key}")
                .AppendLine($"    {_Problem.Problem}")
                .AppendLine($"    {_Problem.Fix}")
                .AppendLine();
        }

        return _Message
            .Append("README.md has the full setup, including how to create the client application row.")
            .ToString();
    }

    private static RequiredSetting? CheckApiBaseUrl(IConfiguration configuration)
    {
        const string _Key = "apiBaseUrl";
        const string _Fix = $"dotnet user-secrets set \"{_Key}\" \"http://localhost:57175\" --project Home.WebUI";

        var _Value = configuration[_Key];

        if (string.IsNullOrWhiteSpace(_Value))
            return new(_Key, "Not set. It is the origin the API is listening on, with no path.", _Fix);

        // The scheme has to be checked separately. "localhost:57175" parses as an absolute URI
        // perfectly happily, with "localhost" as its scheme and "57175" as its path, and then
        // fails much later when HttpClient is asked to send to it.
        if (!Uri.TryCreate(_Value, UriKind.Absolute, out var _Uri)
            || (_Uri.Scheme != Uri.UriSchemeHttp && _Uri.Scheme != Uri.UriSchemeHttps))
            return new(_Key, $"'{_Value}' is not an absolute http or https URL.", _Fix);

        return _Uri.AbsolutePath.TrimEnd('/').EndsWith(DuplicatedApiSegment, StringComparison.OrdinalIgnoreCase)
            ? new(
                _Key,
                $"'{_Value}' ends in /api. Every route is already prefixed with api, so this would call .../api/api/Recipes and 404 on everything.",
                _Fix)
            : null;
    }

    private static RequiredSetting? CheckClientID(IConfiguration configuration)
    {
        const string _Key = "OAuth:AccessToken:ClientID";

        // Parsed by hand rather than through GetValue<long?>, which throws on a value that is not
        // a number at all instead of reporting it alongside everything else that is wrong.
        return long.TryParse(configuration[_Key], out var _ClientID) && _ClientID > 0
            ? null
            : new(
                _Key,
                "Not set, or not a positive number. It is the ClientApplicationID of your row in home.ClientApplication, and it defaults to 1 in appsettings.json.",
                $"dotnet user-secrets set \"{_Key}\" \"1\" --project Home.WebUI");
    }

    private static RequiredSetting? CheckRequiredString(IConfiguration configuration, string key, string problem)
        => string.IsNullOrWhiteSpace(configuration[key])
            ? new(key, problem, $"dotnet user-secrets set \"{key}\" \"<value>\" --project Home.WebUI")
            : null;

    /// <summary>
    /// Throws with every problem listed, or returns quietly.
    /// </summary>
    public static void Validate(IConfiguration configuration)
    {
        var _Problems = Inspect(configuration);

        if (_Problems.Count > 0)
            throw new InvalidOperationException(BuildMessage(_Problems));
    }

    /// <summary>
    /// The check itself, separated from throwing so it can be asserted on directly.
    /// </summary>
    public static IReadOnlyList<RequiredSetting> Inspect(IConfiguration configuration)
        => (List<RequiredSetting>)
        [
            .. new[]
            {
                CheckApiBaseUrl(configuration),
                CheckClientID(configuration),
                CheckRequiredString(
                    configuration,
                    "OAuth:AccessToken:AccessToken",
                    "Not set. It has to match the AccessToken column of your row in home.ClientApplication."),
                CheckRequiredString(
                    configuration,
                    "OAuth:AccessToken:ClientSecret",
                    "Not set. It has to match the Secret column of your row in home.ClientApplication."),
                CheckRequiredString(
                    configuration,
                    "OAuth:AccessToken:GrantType",
                    "Not set. It should be 'password', and appsettings.json sets it."),
                CheckRequiredString(
                    configuration,
                    "OAuth:AccessToken:Scope",
                    "Not set. It should be 'WebApp', and appsettings.json sets it.")
            }.OfType<RequiredSetting>()
        ];

    #endregion Methods

}
