using System.Text;

namespace Home.WebApi.Infrastructure.Configuration;

/// <summary>
/// Everything this project needs before it can answer a request, checked once at startup.
/// <para>
/// The connection string used to go straight into <c>UseSqlServer</c> unchecked, so a missing one
/// surfaced later as a provider error from whichever query happened to run first. Naming it here
/// costs one line and saves reading a stack trace.
/// </para>
/// </summary>
public static class RequiredConfiguration
{

    #region Methods

    private static string BuildMessage(IReadOnlyList<RequiredSetting> problems)
    {
        var _Message = new StringBuilder()
            .AppendLine($"Home.WebApi cannot start. {problems.Count} setting{(problems.Count == 1 ? " is" : "s are")} missing or invalid.")
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
            .Append("README.md has the full setup.")
            .ToString();
    }

    /// <summary>
    /// The check itself, separated from throwing so it can be asserted on directly.
    /// </summary>
    public static IReadOnlyList<RequiredSetting> Inspect(IConfiguration configuration)
    {
        const string _Key = "databaseConnectionString";

        return string.IsNullOrWhiteSpace(configuration[_Key])
            ? [new(
                _Key,
                "Not set. There is no appsettings default for this on purpose, because it is a secret.",
                $"dotnet user-secrets set \"{_Key}\" \"Server=(localdb)\\MSSQLLocalDB;Database=Home;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True\" --project Home.WebApi")]
            : [];
    }

    /// <summary>
    /// Throws with every problem listed, or returns quietly.
    /// </summary>
    public static void Validate(IConfiguration configuration)
    {
        var _Problems = Inspect(configuration);

        if (_Problems.Count > 0)
            throw new InvalidOperationException(BuildMessage(_Problems));
    }

    #endregion Methods

}
