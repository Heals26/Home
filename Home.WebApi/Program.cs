using CleanArchitecture.Mediator.Setup;
using Home.Application.Services.Pipelines;
using Home.Application.Services.Validation;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Home.Persistence.Database;
using Home.WebApi;
using Home.WebApi.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var _Builder = WebApplication.CreateBuilder(args);

ScopedServices(_Builder.Services);
ScrutorServices(_Builder.Services);
AddMediator(_Builder.Services);

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

static void Configure(IApplicationBuilder app, IWebHostEnvironment environment, PersistenceContext persistenceContext)
{
    app.UseStaticFiles();
    app.UseSwagger();
    _ = app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v0/swagger.json", "Unversioned");
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Liink v1");
        options.EnableFilter();
        options.DocumentTitle = "Liink Swagger";
    });
    _ = app.UseDeveloperExceptionPage();

    app.UseApiAuditing();
    app.UseAuthentication();
    //app.UseAuthorization();

    app.Use(async (context, next) =>
    {
        context.Request.EnableBuffering();
        await next();
    });

    app.UseEndpoints(e =>
    {
        e.MapControllers();
    });

    persistenceContext.Database.Migrate();
}

static void ScopedServices(IServiceCollection services)
{
    _ = services.AddScoped<IPasswordService, IPasswordService>();
}

static void ScrutorServices(IServiceCollection services)
    => _ = services.Scan(s =>
    {
        _ = s.FromAssemblies(AssemblyUtility.GetAssembly())
        .AddClasses(c => c.AssignableTo(typeof(IAuditLogic<>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime();
    });
