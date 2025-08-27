using CleanArchitecture.Mediator.Setup;
using Home.Application.Services.Pipelines;
using Home.Application.Services.Validation;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Home.WebApi;

var _Builder = WebApplication.CreateBuilder(args);

ScopedServices(_Builder.Services);
ScrutorServices(_Builder.Services);
AddMediator(_Builder.Services);

static void ScrutorServices(IServiceCollection services)
    => _ = services.Scan(s =>
    {
        _ = s.FromAssemblies(AssemblyUtility.GetAssembly())
        .AddClasses(c => c.AssignableTo(typeof(IAuditLogic<>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime();
    });

static void ScopedServices(IServiceCollection services)
{
    _ = services.AddScoped<IPasswordService, IPasswordService>();
}

static IServiceCollection AddMediator(IServiceCollection services)
{
    CleanArchitectureMediator.Setup(config =>
    {
        _ = config.AddPipeline<DefaultPipeline>(pipeline
            => pipeline.AddPipe(async (inputPort, outputPort, serviceFactory, nextPipeHandleAsync, cancellationToken) =>
            {
                await nextPipeHandleAsync();
            })
            .AddAuthentication(AuthenticationMode.MultiPrincipal)
            .AddAuthorisationPolicyValidation<HomeAuthorisationFailure>()
            .AddInputPortValidation<HomeInputPortValidationFailure>()
            .AddBusinessRuleEvaluation()
            .AddInteractorInvocation());
    }, registration => registration
        .AddAssemblies(Home.Application.AssemblyUtility.GetAssembly())
        .WithSingletonInstanceRegistrationAction((serviceType, instance) => services.AddSingleton(serviceType, instance))
        .WithSingletonServiceRegistrationAction((serviceType, implementationType) => services.AddSingleton(serviceType, implementationType)));

    return services;
}
