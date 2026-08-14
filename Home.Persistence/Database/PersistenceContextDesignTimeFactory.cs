using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Home.Persistence.Database;

/// <summary>
/// Lets <c>dotnet ef</c> build the model with this project as the startup project, so migrations
/// can be added while the API is running and holding its output folder locked. Adding a migration
/// never opens a connection — the string only needs to be well-formed. <c>database update</c>
/// still goes through <c>Home.WebApi</c> and its user secrets, or set
/// <c>HOME_DESIGNTIME_CONNECTIONSTRING</c> to target a real database from here.
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
