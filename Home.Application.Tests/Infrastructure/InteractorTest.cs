using AutoMapper;
using FluentAssertions;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure.Mapping;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.Infrastructure;

/// <summary>
/// The shared arrangement for a use case test: a real database, two households sharing it, and the
/// real presenter on the other end.
/// <para>
/// Driving the presenter rather than a mocked output port is deliberate. The fault these tests
/// exist to catch is a presenter reading a navigation the interactor never projected, which a
/// mocked port cannot see because it never reads anything. Running the real one turns that into a
/// failing test instead of a five hundred in front of the family.
/// </para>
/// <para>
/// A neighbouring household is seeded alongside ours in every scenario, because household
/// isolation is an invariant of every query in this application (14 Aug) and an untested one is
/// only a convention.
/// </para>
/// </summary>
public abstract class InteractorTest : IDisposable
{

    #region Constants

    /// <summary>
    /// The household the test is signed in as. Its rows are numbered from 100, so a leak between
    /// households names itself in the failure message rather than reading as an off-by-one.
    /// </summary>
    protected const long OurHouseholdID = 1;

    /// <summary>
    /// Another family sharing the same database. Its rows are numbered from 900.
    /// </summary>
    protected const long TheirHouseholdID = 2;

    #endregion Constants

    #region Fields

    /// <summary>
    /// Built once for the assembly: assembling the AutoMapper configuration costs more than every
    /// test in a class put together.
    /// </summary>
    private static readonly Lazy<IMapper> s_Mapper = new(TestMapper.Create, LazyThreadSafetyMode.ExecutionAndPublication);

    #endregion Fields

    #region Constructors

    protected InteractorTest()
    {
        this.Ours = new Household() { HouseholdID = OurHouseholdID, Name = "Ours" };
        this.Theirs = new Household() { HouseholdID = TheirHouseholdID, Name = "Theirs" };

        this.Member = new User()
        {
            UserID = 100,
            Email = "member@ours.test",
            FirstName = "Ada",
            Household = this.Ours,
            LastName = "Member"
        };

        this.Neighbour = new User()
        {
            UserID = 900,
            Email = "neighbour@theirs.test",
            FirstName = "Bo",
            Household = this.Theirs,
            LastName = "Neighbour"
        };

        this.SignedInHousehold = this.Ours;
        this.SignedInUser = this.Member;

        _ = this.AuthorisationService.Setup(a => a.GetHousehold()).Returns(() => this.SignedInHousehold);
        _ = this.AuthorisationService.Setup(a => a.GetUser()).Returns(() => this.SignedInUser);
    }

    #endregion Constructors

    #region Properties

    protected Mock<IAuthorisationService> AuthorisationService { get; } = new();

    protected TestDatabase Database { get; } = new();

    protected static IMapper Mapper
        => s_Mapper.Value;

    /// <summary>A member of our household.</summary>
    protected User Member { get; }

    /// <summary>A member of the other household, who should never appear in our results.</summary>
    protected User Neighbour { get; }

    protected Household Ours { get; }

    /// <summary>
    /// Who the interactor believes it is running for. Reassign to run the same scenario from the
    /// other household's side.
    /// </summary>
    protected Household SignedInHousehold { get; set; }

    protected User SignedInUser { get; set; }

    protected Household Theirs { get; }

    #endregion Properties

    #region Methods

    public void Dispose()
    {
        this.Database.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// The response a presenter produced, having first insisted it produced one. A presenter that
    /// dereferences an unloaded navigation throws before reaching here, which is the whole point.
    /// </summary>
    protected static TResponse Ok<TResponse>(OutputPortPresenter presenter)
    {
        _ = presenter.PresentedSuccessfully.Should().BeTrue("the presenter should have presented a result");

        return presenter.Result
            .Should().BeOfType<OkObjectResult>().Which
            .Value.Should().BeOfType<TResponse>().Which;
    }

    /// <summary>
    /// The service factory an interactor resolves from. Each call hands out a context that has
    /// never seen the seeded rows, so a navigation is only ever populated by the query under test.
    /// </summary>
    protected TestServiceFactory Services()
        => new TestServiceFactory()
            .With(this.Database.Read())
            .With(this.AuthorisationService.Object);

    protected static void ShouldBeNotFound(OutputPortPresenter presenter)
    {
        _ = presenter.PresentedSuccessfully.Should().BeFalse("a missing entity is not a successful presentation");
        _ = presenter.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion Methods

}
