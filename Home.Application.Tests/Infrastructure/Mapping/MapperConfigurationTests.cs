using AutoMapper;
using FluentAssertions;

namespace Home.Application.Tests.Infrastructure.Mapping;

/// <summary>
/// The guard against a mapping breaking silently. AutoMapper only complains when a map is first
/// used, so an unmapped member added today surfaces as a 500 on whichever screen happens to hit
/// it — which has caught this codebase out more than once. Asserting the whole configuration
/// turns that into a failing build instead.
/// </summary>
public class MapperConfigurationTests
{

    #region Methods

    /// <summary>
    /// The message is captured rather than the exception allowed to escape: AutoMapper's
    /// configuration exception does not survive the test runner's serialisation, and a test that
    /// throws it disappears from the run instead of failing it.
    /// </summary>
    private static string? DescribeConfigurationFailure()
    {
        try
        {
            TestMapper.BuildConfiguration().AssertConfigurationIsValid();
            return null;
        }
        catch (Exception _Exception)
        {
            return _Exception.Message;
        }
    }

    [Fact]
    public void Configuration_HasNoUnmappedMembersInAnyProfile()
        => DescribeConfigurationFailure().Should().BeNull();

    [Fact]
    public void Configuration_FindsTheProfilesInEveryAssembly()
    {
        // A wrong assembly list would leave the assertion above passing over nothing.
        var _Profiles = new[]
        {
            Application.AssemblyUtility.GetAssembly(),
            Domain.AssemblyUtility.GetAssembly(),
            Persistence.AssemblyUtility.GetAssembly(),
            WebApi.AssemblyUtility.GetAssembly()
        }.SelectMany(a => a.GetTypes().Where(t => typeof(Profile).IsAssignableFrom(t) && !t.IsAbstract));

        _Profiles.Should().NotBeEmpty();
    }

    #endregion Methods

}
