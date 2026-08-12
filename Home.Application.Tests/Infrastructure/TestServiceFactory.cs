using CleanArchitecture.Mediator;

namespace Home.Application.Tests.Infrastructure;

/// <summary>
/// Builds the <see cref="ServiceFactory"/> delegate that interactors resolve their dependencies
/// from. Interactors have no constructor, so this is the only seam a test has.
/// </summary>
internal class TestServiceFactory
{

    #region Fields

    private readonly Dictionary<Type, object> m_Services = [];

    #endregion Fields

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
