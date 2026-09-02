using CleanArchitecture.Mediator;
using Microsoft.Extensions.Time.Testing;

namespace Home.Application.Tests.Infrastructure;

/// <summary>
/// Builds the <see cref="ServiceFactory"/> delegate that interactors resolve their dependencies
/// from. Interactors have no constructor, so this is the only seam a test has.
/// <para>
/// A <see cref="FakeTimeProvider"/> is registered by default, so anything reading the clock gets a
/// fixed, controllable time without every test having to wire one up.
/// </para>
/// </summary>
public class TestServiceFactory
{

    #region Constants

    /// <summary>
    /// An arbitrary but fixed instant. Chosen as a Wednesday so day-of-week logic has somewhere
    /// unremarkable to stand.
    /// </summary>
    public static readonly DateTimeOffset DefaultNow = new(2026, 8, 12, 9, 30, 0, TimeSpan.Zero);

    #endregion Constants

    #region Fields

    private readonly Dictionary<Type, object> m_Services = [];

    #endregion Fields

    #region Constructors

    public TestServiceFactory()
    {
        this.Time = new FakeTimeProvider(DefaultNow);
        this.m_Services[typeof(TimeProvider)] = this.Time;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// The clock every resolved service will see. Advance or set it before invoking the interactor.
    /// </summary>
    public FakeTimeProvider Time { get; }

    #endregion Properties

    #region Methods

    public TestServiceFactory With<TService>(TService service) where TService : class
    {
        this.m_Services[typeof(TService)] = service;
        return this;
    }

    public ServiceFactory Build()
        => type => this.m_Services.TryGetValue(type, out var _Service) ? _Service : null!;

    #endregion Methods

}
