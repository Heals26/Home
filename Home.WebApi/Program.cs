using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Home.WebApi;

var _Builder = WebApplication.CreateBuilder(args);

ScopedServices(_Builder.Services);
ScrutorServices(_Builder.Services);


static void ScrutorServices(IServiceCollection services)
{
    _ = services.Scan(s =>
    {
        _ = s.FromAssemblies(AssemblyUtility.GetAssembly())
        .AddClasses(c => c.AssignableTo(typeof(IAuditLogic<>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime();
    });
}

static void ScopedServices(IServiceCollection services)
{
    _ = services.AddScoped<IPasswordService, IPasswordService>();
}

