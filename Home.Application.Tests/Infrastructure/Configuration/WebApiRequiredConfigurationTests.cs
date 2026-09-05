using FluentAssertions;
using Home.WebApi.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace Home.Application.Tests.Infrastructure.Configuration;

/// <summary>
/// The startup check on the API's settings. The connection string used to go straight into
/// <c>UseSqlServer</c> unchecked, so a missing one surfaced later as a provider error from
/// whichever query happened to run first.
/// </summary>
public class WebApiRequiredConfigurationTests
{

    #region Methods

    private static IConfiguration BuildConfiguration(string? databaseConnectionString)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>() { ["databaseConnectionString"] = databaseConnectionString })
            .Build();

    [Fact]
    public void Inspect_WhenTheConnectionStringIsSet_FindsNothingWrong()
        => RequiredConfiguration.Inspect(BuildConfiguration("Server=(localdb)\\MSSQLLocalDB;Database=Home;Trusted_Connection=True"))
            .Should().BeEmpty();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Inspect_WhenTheConnectionStringIsMissing_NamesItAndHowToSetIt(string? databaseConnectionString)
    {
        var _Problem = RequiredConfiguration.Inspect(BuildConfiguration(databaseConnectionString)).Should().ContainSingle().Subject;

        _ = _Problem.Key.Should().Be("databaseConnectionString");
        _ = _Problem.Fix.Should().Contain("dotnet user-secrets set").And.Contain("--project Home.WebApi");
    }

    [Fact]
    public void Validate_WhenTheConnectionStringIsMissing_ThrowsNamingIt()
    {
        var _Validate = () => RequiredConfiguration.Validate(BuildConfiguration(null));

        _ = _Validate.Should().Throw<InvalidOperationException>()
            .Which.Message.Should()
                .Contain("databaseConnectionString").And
                .Contain("README.md");
    }

    [Fact]
    public void Validate_WhenTheConnectionStringIsSet_DoesNotThrow()
    {
        var _Validate = () => RequiredConfiguration.Validate(BuildConfiguration("Server=."));

        _ = _Validate.Should().NotThrow();
    }

    #endregion Methods

}
