using Asp.Versioning;
using CleanArchitecture.Mediator.Setup;
using Home.Application.Services.Database;
using Home.Application.Services.Persistence;
using Home.Application.Services.Pipelines;
using Home.Application.Services.Validation;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Home.Persistence.Database;
using Home.WebApi;
using Home.WebApi.Infrastructure.Extensions;
using Home.WebApi.Infrastructure.Filters;
using Microsoft.EntityFrameworkCore;
using System.Text;

var _Builder = WebApplication.CreateBuilder(args);

ScopedServices(_Builder.Services);
ScrutorServices(_Builder.Services);
SetupMediator(_Builder.Services);
SetupInfrastructure(_Builder.Services);
SetupEntityFramework(_Builder.Services, _Builder.Configuration);

var _Application = _Builder.Build();

SetupApplication(_Application, _Builder.Environment);

static void SetupApplication(WebApplication app, IWebHostEnvironment environment)
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

    using var _Scope = app.Services.CreateScope();
    var _PersistenceContext = _Scope.ServiceProvider.GetRequiredService<PersistenceContext>();
    _PersistenceContext.Database.Migrate();
}

static IServiceCollection SetupEntityFramework(IServiceCollection services, IConfiguration configuration)
{
    var _ConnectionString = configuration.GetConnectionString("databaseConnectionString");
    _ = services.AddDbContext<IPersistenceContext, PersistenceContext>(options =>
    {
        //_ = options.UseSqlServer($"Server={_Credentials.ServerName}; Database={_Credentials.DatabaseName}; User ID={_Credentials.UserName}; Password={_Credentials.Password};", o =>
        _ = options.UseSqlServer(_ConnectionString, o =>
        {
            _ = o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            _ = o.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
        })
        .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: true);
    });

    _ = services.AddDbContext<IAuditPersistenceContext, AuditPersistenceContext>(options =>
    {
        //_ = options.UseSqlServer($"Server={_Credentials.ServerName}; Database={_Credentials.DatabaseName}; User ID={_Credentials.UserName}; Password={_Credentials.Password};", o =>
        _ = options.UseSqlServer(_ConnectionString, o =>
        {
            _ = o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            _ = o.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
        })
        .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: true);
    });

    return services;
}

static IServiceCollection SetupInfrastructure(IServiceCollection services)
{
    services.AddAutoMapper(cfg => { },
        Home.Application.AssemblyUtility.GetAssembly(),
        Home.Domain.AssemblyUtility.GetAssembly(),
        Home.Persistence.AssemblyUtility.GetAssembly(),
        Home.WebApi.AssemblyUtility.GetAssembly());

    services.AddApiVersioning(options =>
    {
        options.ApiVersionReader = new HeaderApiVersionReader(ApiVersionHeaderFilter.API_HEADER);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    });

    //services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");

    var _UnversionedDescription = new StringBuilder();
    _UnversionedDescription.AppendLine("<p style=\"color: red\">");
    _UnversionedDescription.AppendLine("This is the Unversioned definition of the API, and may change at any time.");
    _UnversionedDescription.AppendLine("<br>");
    _UnversionedDescription.AppendLine("For compatibility and stability, it is recommended to develop against a specific version definition.");
    _UnversionedDescription.AppendLine("</p>");

    var _VersionDescription = new StringBuilder();
    _VersionDescription.AppendLine("<strong>Liink is the software solution for the construction industry. Liink provides vital safety, production and construction collaboration between builders, subcontractors, and suppliers</strong>");
    _VersionDescription.AppendLine("<br />");
    _VersionDescription.AppendLine("To use this version of the API include the following header with your request:");
    _VersionDescription.AppendLine("<br>");
    _VersionDescription.AppendLine("<strong>key:</strong> `api-version`");
    _VersionDescription.AppendLine("<br>");
    _VersionDescription.AppendLine("<strong>value:</strong> `<Version>`");

    services.AddSwaggerGen(s =>
    {
        s.OperationFilter<ApiVersionHeaderFilter>();

        s.CustomSchemaIds(t => t.FullName);

        s.SwaggerDoc("v0", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Liink", Version = "v0", Description = _UnversionedDescription.ToString() });
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

static IServiceCollection ScopedServices(IServiceCollection services)
{
    _ = services.AddScoped<IPasswordService, IPasswordService>();

    return services;
}

static IServiceCollection ScrutorServices(IServiceCollection services)
{
    _ = services.Scan(s =>
    {
        _ = s.FromAssemblies(AssemblyUtility.GetAssembly())
        .AddClasses(c => c.AssignableTo(typeof(IAuditLogic<>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime();
    });

    return services;
}
