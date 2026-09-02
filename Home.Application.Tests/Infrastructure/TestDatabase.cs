using Home.Application.Services.Persistence;
using Home.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Tests.Infrastructure;

/// <summary>
/// A real <see cref="PersistenceContext"/> over a private in-memory store, so an interactor under
/// test runs through EF's own query pipeline rather than LINQ over a list.
/// <para>
/// That distinction is the entire point of this class. A mocked <see cref="IPersistenceContext"/>
/// returning <c>AsQueryable()</c> hands the interactor an object graph that is already fully
/// connected, so a projection which forgets to name a navigation still passes — the navigation was
/// never unloaded to begin with. Against a real context the projection is what decides, and one
/// that forgets a navigation comes back null exactly as it does in production. Three screens have
/// shipped broken that way; a mock cannot catch a fourth.
/// </para>
/// <para>
/// Seeding and reading therefore use separate contexts. Entities left tracked by the seed would be
/// fixed up onto the query result and hide the same fault a second way.
/// </para>
/// </summary>
public sealed class TestDatabase : IDisposable
{

    #region Fields

    private readonly List<PersistenceContext> m_ReadContexts = [];
    private readonly string m_StoreName = Guid.NewGuid().ToString();

    private PersistenceContext? m_SeedContext;

    #endregion Fields

    #region Methods

    public void Dispose()
    {
        foreach (var _Context in this.m_ReadContexts)
            _Context.Dispose();

        this.m_ReadContexts.Clear();

        this.m_SeedContext?.Dispose();
        this.m_SeedContext = null;
    }

    private PersistenceContext NewContext()
        => new(new DbContextOptionsBuilder<PersistenceContext>()
            .UseInMemoryDatabase(this.m_StoreName)
            .EnableSensitiveDataLogging()
            .Options);

    /// <summary>
    /// A context that has never seen the seeded entities, so a navigation is populated only
    /// because the query under test asked for it.
    /// </summary>
    public IPersistenceContext Read()
    {
        var _Context = this.NewContext();
        this.m_ReadContexts.Add(_Context);

        return _Context;
    }

    /// <summary>
    /// Writes the given roots, letting EF cascade to everything they reference — seeding an
    /// activity brings its household, sections, regions and lines with it.
    /// <para>
    /// One context does all the seeding for the life of the database, so a second call knows what
    /// the first already inserted and a shared entity is not offered twice.
    /// </para>
    /// </summary>
    public TestDatabase Seed(params object[] roots)
    {
        this.m_SeedContext ??= this.NewContext();

        this.m_SeedContext.AddRange(roots);
        _ = this.m_SeedContext.SaveChanges();

        return this;
    }

    #endregion Methods

}
