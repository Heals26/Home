using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Home.Persistence.Database;

/// <summary>
/// Lets <c>dotnet ef</c> build the model with this project as the startup project, so migrations
/// can be added while the API is running and holding its output folder locked. Adding a migration
/// never opens a connection — the string only needs to be well-formed.
/// <para>
/// <b>This factory wins over the startup project.</b> EF prefers an
/// <see cref="IDesignTimeDbContextFactory{TContext}"/> to the host's service provider, so
/// <c>database update</c> comes here even with <c>--startup-project Home.WebApi</c> and never
/// reads the API's user secrets. Set <c>HOME_DESIGNTIME_CONNECTIONSTRING</c> to the database you
/// mean, or the update silently lands on the LocalDB fallback below instead of the real one.
/// </para>
/// </summary>
public class PersistenceContextDesignTimeFactory : IDesignTimeDbContextFactory<PersistenceContext>
{

    #region Methods

    public PersistenceContext CreateDbContext(string[] args)
    {
        var _ConnectionString = Environment.GetEnvironmentVariable("HOME_DESIGNTIME_CONNECTIONSTRING")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=Home;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var _Options = new DbContextOptionsBuilder<PersistenceContext>();
        _ = _Options.UseSqlServer(_ConnectionString, o => _ = o.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));

        return new PersistenceContext(_Options.Options);
    }

    #endregion Methods

}
