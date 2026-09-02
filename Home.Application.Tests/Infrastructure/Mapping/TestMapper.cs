using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Home.Application.Tests.Infrastructure.Mapping;

/// <summary>
/// The single place the AutoMapper profile assemblies are listed for tests. Mirrors the
/// registration in <c>Home.WebApi/Program.cs</c>; if an assembly is added there it belongs here
/// too, or its profiles go unchecked by <see cref="MapperConfigurationTests"/> and unavailable to
/// the presenters driven by <see cref="InteractorTest"/>.
/// </summary>
internal static class TestMapper
{

    #region Methods

    public static MapperConfiguration BuildConfiguration()
        => new(
            cfg => cfg.AddMaps(
                Application.AssemblyUtility.GetAssembly(),
                Domain.AssemblyUtility.GetAssembly(),
                Persistence.AssemblyUtility.GetAssembly(),
                WebApi.AssemblyUtility.GetAssembly()),
            NullLoggerFactory.Instance);

    public static IMapper Create()
        => BuildConfiguration().CreateMapper();

    #endregion Methods

}
