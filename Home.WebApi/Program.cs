using Asp.Versioning;
using CleanArchitecture.Mediator;
using CleanArchitecture.Mediator.Setup;
using Home.Application.Infrastructure.Users;
using Home.Application.Services.Persistence;
using Home.Application.Services.Validation;
using Home.Application.UseCases.ApiAuditing;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Home.Persistence.Database;
using Home.WebApi;
using Home.WebApi.Infrastructure.Extensions;
using Home.WebApi.Infrastructure.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

var _Builder = WebApplication.CreateBuilder(args);

SetupScopedServices(_Builder.Services);
SetupScrutorServices(_Builder.Services);
SetupSecrets(_Builder);
SetupMediator(_Builder.Services);
SetupInfrastructure(_Builder.Services);
SetupEntityFramework(_Builder.Services, _Builder.Configuration, _Builder.Environment);

var _Application = _Builder.Build();

SetupApplication(_Application, _Builder.Environment);

_Application.Run();

static void SetupApplication(WebApplication app, IWebHostEnvironment environment)
{
    _ = app.UseStaticFiles();
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Home v1");
        options.EnableFilter();
        options.DocumentTitle = "Liink Swagger";
    });
    _ = app.UseDeveloperExceptionPage();

    _ = app.UseApiAuditing();
    _ = app.UseAuthentication();

    _ = app.UseRouting();

    //app.UseAuthorization();

    _ = app.Use(async (context, next) =>
    {
        context.Request.EnableBuffering();
        await next();
    });

    _ = app.UseEndpoints(e =>
    {
        _ = e.MapControllers();
    });

    using var _Scope = app.Services.CreateScope();
    var _PersistenceContext = _Scope.ServiceProvider.GetRequiredService<PersistenceContext>();
    _PersistenceContext.Database.Migrate();
}

static IServiceCollection SetupEntityFramework(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    var _ConnectionString = configuration["databaseConnectionString"];

    if (environment.IsEnvironment("Tablet"))
    {
        _ = services.AddDbContext<IPersistenceContext, PersistenceContext>(options =>
        {
            _ = options.UseSqlite(_ConnectionString, o =>
            {
                _ = o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                _ = o.MigrationsHistoryTable("__EFMigrationsHistory");
            })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: true);
        });
        _ = services.AddDbContext<IAuditPersistenceContext, AuditPersistenceContext>(options =>
        {
            _ = options.UseSqlite(_ConnectionString, o =>
            {
                _ = o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                _ = o.MigrationsHistoryTable("__EFMigrationsHistory");
            })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: true);
        });
    }
    else
    {
        _ = services.AddDbContext<IPersistenceContext, PersistenceContext>(options =>
        {
            _ = options.UseSqlServer(_ConnectionString, o =>
            {
                _ = o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                _ = o.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
            })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: true);
        });

        _ = services.AddDbContext<IAuditPersistenceContext, AuditPersistenceContext>(options =>
        {
            _ = options.UseSqlServer(_ConnectionString, o =>
            {
                _ = o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                _ = o.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
            })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: true);
        });
    }

    return services;
}

static IServiceCollection SetupInfrastructure(IServiceCollection services)
{
    _ = services.AddControllers();

    _ = services.AddAutoMapper(cfg => { },
        Home.Application.AssemblyUtility.GetAssembly(),
        Home.Domain.AssemblyUtility.GetAssembly(),
        Home.Persistence.AssemblyUtility.GetAssembly(),
        Home.WebApi.AssemblyUtility.GetAssembly());

    _ = services.AddApiVersioning(options =>
    {
        options.ApiVersionReader = new HeaderApiVersionReader(ApiVersionHeaderFilter.API_HEADER);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    });

    //services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");

    var _UnversionedDescription = new StringBuilder();
    _ = _UnversionedDescription.AppendLine("<p style=\"color: red\">");
    _ = _UnversionedDescription.AppendLine("This is the Unversioned definition of the API, and may change at any time.");
    _ = _UnversionedDescription.AppendLine("<br>");
    _ = _UnversionedDescription.AppendLine("For compatibility and stability, it is recommended to develop against a specific version definition.");
    _ = _UnversionedDescription.AppendLine("</p>");

    var _VersionDescription = new StringBuilder();
    _ = _VersionDescription.AppendLine("<strong>Liink is the software solution for the construction industry. Liink provides vital safety, production and construction collaboration between builders, subcontractors, and suppliers</strong>");
    _ = _VersionDescription.AppendLine("<br />");
    _ = _VersionDescription.AppendLine("To use this version of the API include the following header with your request:");
    _ = _VersionDescription.AppendLine("<br>");
    _ = _VersionDescription.AppendLine("<strong>key:</strong> `api-version`");
    _ = _VersionDescription.AppendLine("<br>");
    _ = _VersionDescription.AppendLine("<strong>value:</strong> `<Version>`");

    _ = services.AddSwaggerGen(s =>
    {
        s.OperationFilter<ApiVersionHeaderFilter>();

        s.CustomSchemaIds(t => t.FullName);

        s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Liink", Version = "v1", Description = _VersionDescription.ToString().Replace("<Version>", "1.0") });

        var _PresentationXML = $"{Home.WebApi.AssemblyUtility.GetAssembly().GetName().Name}.xml";
        s.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, _PresentationXML));
    });

    return services;
}

static IServiceCollection SetupMediator(IServiceCollection services)
{
    CleanArchitectureMediator.Setup(config =>
    {
        _ = config.AddPipeline<Pipeline>(pipeline
            => pipeline.AddPipe(async (inputPort, outputPort, serviceFactory, nextPipeHandleAsync, cancellationToken) =>
            {
                await nextPipeHandleAsync();
            })
            //.AddAuthentication(AuthenticationMode.MultiPrincipal)
            //.AddAuthorisationPolicyValidation<HomeAuthorisationFailure>()
            //.AddLicencePolicyValidation<HomeLicencePolicyFailure>()
            .AddInputPortValidation<HomeInputPortValidationFailure>()
            .AddBusinessRuleEvaluation()
            .AddInteractorInvocation());
    }, registration => registration
        .AddAssemblies(Home.Application.AssemblyUtility.GetAssembly())
        .WithSingletonInstanceRegistrationAction((serviceType, instance) => services.AddSingleton(serviceType, instance))
        .WithSingletonServiceRegistrationAction((serviceType, implementationType) => services.AddSingleton(serviceType, implementationType)));

    _ = services.AddScoped<ServiceFactory>(s => s.GetService);

    return services;
}

static IServiceCollection SetupScopedServices(IServiceCollection services)
{
    _ = services.AddScoped<IPasswordService, PasswordService>();
    _ = services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    _ = services.AddScoped<CreateApiAuditEntry>();

    return services;
}

static void SetupSecrets(WebApplicationBuilder builder)
    => builder.Configuration.AddUserSecrets<Program>(false, true);

static IServiceCollection SetupScrutorServices(IServiceCollection services)
{
    _ = services.Scan(s =>
    {
        _ = s.FromAssemblies(Home.Domain.AssemblyUtility.GetAssembly(), Home.Application.AssemblyUtility.GetAssembly())
        .AddClasses(c => c.AssignableTo(typeof(IAuditLogic<>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime();
    });

    _ = services.Scan(s =>
    {
        _ = s.FromAssemblies(AssemblyUtility.GetAssembly())
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Presenter")))
            .AsSelf()
            .WithScopedLifetime();
    });

    return services;
}
