using Asp.Versioning;
using CleanArchitecture.Mediator;
using CleanArchitecture.Mediator.Setup;
using Home.Application.Infrastructure.Security;
using Home.Application.Infrastructure.Activities;
using Home.Application.Infrastructure.Households;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Infrastructure.Users;
using Home.Application.Infrastructure.Lights;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.EntityLogic.Households;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.EntityLogic.Recipes;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.RecipeImports;
using Home.Application.Services.Security;
using Home.Application.Services.Validation;
using Home.Application.UseCases.ApiAuditing;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Home.Persistence.Database;
using Home.WebApi;
using Home.WebApi.Infrastructure.AutoMapper.Resolvers;
using Home.WebApi.Infrastructure.ChangeNotifications;
using Home.WebApi.Infrastructure.Extensions;
using Home.WebApi.Infrastructure.Filters;
using Home.WebApi.Infrastructure.Lights;
using Home.WebApi.Infrastructure.OAuth;
using Home.WebApi.Infrastructure.RecipeImports;
using Home.WebApi.Infrastructure.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;

var _Builder = WebApplication.CreateBuilder(args);

SetupLogging(_Builder.Logging);
SetupScopedServices(_Builder.Services);
SetupScrutorServices(_Builder.Services);
SetupSecrets(_Builder);
SetupMediator(_Builder.Services);
SetupInfrastructure(_Builder.Services);
SetupLights(_Builder.Services, _Builder.Configuration);
SetupRecipeImports(_Builder.Services);
SetupEntityFramework(_Builder.Services, _Builder.Configuration);

SetupAuthentication(_Builder.Services);
SetupAuthorisation(_Builder.Services);

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
        options.DocumentTitle = "Home Swagger";
    });
    _ = app.UseDeveloperExceptionPage();

    _ = app.UseRouting();
    _ = app.UseApiAuditing();
    _ = app.UseAuthentication();
    _ = app.UseAuthorization();

    _ = app.UseEndpoints(e =>
    {
        _ = e.MapControllers();
        _ = e.MapHub<ChangeNotificationsHub>("/hubs/changes");
    });

    using var _Scope = app.Services.CreateScope();
    var _PersistenceContext = _Scope.ServiceProvider.GetRequiredService<PersistenceContext>();
    _PersistenceContext.Database.Migrate();

    SeedLookups(_PersistenceContext);
}

// Board columns are no longer seeded here: they belong to a household now, so a global row
// would be unreachable by every scoped query. RegisterHousehold seeds them per household
// through IHouseholdSetupLogic, and the migration backfilled the existing ones.
static void SeedLookups(PersistenceContext context)
{
    if (!context.Set<ActivityStatus>().Any())
        context.AddRange(
            new ActivityStatus() { Name = "Todo" },
            new ActivityStatus() { Name = "In Progress" },
            new ActivityStatus() { Name = "Done" });

    _ = context.SaveChanges();
}

static IServiceCollection SetupAuthentication(IServiceCollection services)
{
    _ = services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = FrameworkValues.Flexible;
        o.DefaultChallengeScheme = FrameworkValues.Flexible;
    }).AddPolicyScheme(FrameworkValues.Flexible, FrameworkValues.Flexible, o =>
    {
        o.ForwardDefaultSelector = context =>
        {
            _ = context.Request.Headers.TryGetValue(FrameworkValues.Authorisation, out var _AuthorisationHeaderValue);

            if (_AuthorisationHeaderValue.Count != 0)

                if (_AuthorisationHeaderValue.Single().StartsWith(FrameworkValues.Basic))
                    return FrameworkValues.Basic;
                else if (_AuthorisationHeaderValue.Single().StartsWith(FrameworkValues.Bearer))
                    return FrameworkValues.Bearer;

            return FrameworkValues.Bearer;
        };
    })
    .AddBasicAuthentication()
    .AddBearerAuthentication();

    return services;
}

static IServiceCollection SetupAuthorisation(IServiceCollection services)
{
    _ = services.AddScoped<IAuthorizationHandler, ScopeHandler>()
        .AddScoped<IAuthorizationHandler, WebAppPlatformHandler>();

    _ = services.AddAuthorization(o =>
    {
        var _PolicyBuilder = new AuthorizationPolicyBuilder(FrameworkValues.Bearer, FrameworkValues.Basic).RequireAuthenticatedUser();
        o.DefaultPolicy = _PolicyBuilder.Build();

        o.AddPolicy(FrameworkValues.ScopeWebApp, p => p
            .AddRequirements(new ScopeRequirement(FrameworkValues.ScopeWebApp))
            .AddRequirements(new WebAppPlatformRequirement()));
    });

    return services;
}

// SQL Server everywhere. There used to be a SQLite branch for a "Tablet" environment, but the
// migrations have always been SQL Server-shaped and fail on SQLite, so that path could never
// build a schema. A tablet points at the same server as everything else; LocalDB covers dev.
static IServiceCollection SetupEntityFramework(IServiceCollection services, IConfiguration configuration)
{
    var _ConnectionString = configuration["databaseConnectionString"];

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

    return services;
}

static IServiceCollection SetupInfrastructure(IServiceCollection services)
{
    // The audit filter is what puts an action name against each entry — the middleware alone only
    // ever sees the URI.
    _ = services.AddControllers(o => o.Filters.Add<ApiAuditingActionFilterAttribute>());
    _ = services.AddSignalR();

    // Open generic, so AutoMapper can close it per grant response when it resolves the resolver.
    _ = services.AddTransient(typeof(TokenExpiresInResolver<>));

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
    _ = _VersionDescription.AppendLine("<strong>This is always a work in progress and I am happy to cut corners to avoid burning out.</strong>");
    _ = _VersionDescription.AppendLine("<br />");
    _ = _VersionDescription.AppendLine("Make sure you use the below to be able to connect to the API:");
    _ = _VersionDescription.AppendLine("<br>");
    _ = _VersionDescription.AppendLine("<strong>key:</strong> `api-version`");
    _ = _VersionDescription.AppendLine("<br>");
    _ = _VersionDescription.AppendLine("<strong>value:</strong> `<Version>`");

    _ = services.AddSwaggerGen(s =>
    {
        s.OperationFilter<ApiVersionHeaderFilter>();

        s.CustomSchemaIds(t => t.FullName);

        s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Home", Version = "v1", Description = _VersionDescription.ToString().Replace("<Version>", "1.0") });

        var _PresentationXML = $"{Home.WebApi.AssemblyUtility.GetAssembly().GetName().Name}.xml";
        s.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, _PresentationXML));
    });

    return services;
}

static IServiceCollection SetupLights(IServiceCollection services, IConfiguration configuration)
{
    // No token anywhere is a valid state — the house just has no lights wired up yet. The
    // service reports the provider as unavailable rather than the API failing to start.
    // The token attaches per request via LifxAuthenticationHandler, so the household's stored
    // token (Settings page) wins over the lifxApiToken user secret without a restart.
    _ = services.AddTransient<LifxAuthenticationHandler>();

    _ = services.AddHttpClient<ILightService, LifxLightService>(client =>
    {
        client.BaseAddress = new Uri(LightValues.LifxBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(LightValues.RequestTimeoutSeconds);
    }).AddHttpMessageHandler<LifxAuthenticationHandler>();

    // Schedules only fire while this process is alive — see LightScheduleRunner.
    _ = services.AddHostedService<LightScheduleRunner>();

    // Keeps bulb state fresh so the board notices a wall switch — see LightStateSyncRunner.
    _ = services.AddHostedService<LightStateSyncRunner>();

    return services;
}

// Recipe pages are fetched with an explicit user agent — several big cooking sites refuse
// the default HttpClient one outright. The import only reads embedded JSON-LD, never HTML.
static IServiceCollection SetupRecipeImports(IServiceCollection services)
{
    _ = services.AddHttpClient<IRecipeImportService, JsonLdRecipeImportService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; HomeRecipeImport/1.0)");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html");
    });

    return services;
}

// AutoMapper 15+ is dual-licensed. This project sits well inside the free community tier, and a
// missing licence key changes nothing at runtime other than a startup warning — so filter it out.
static ILoggingBuilder SetupLogging(ILoggingBuilder logging)
    => logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.None);

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

    // The BCL clock abstraction (.NET 8). Interactors resolve this instead of touching
    // DateTime.UtcNow, so tests can drive time with FakeTimeProvider.
    _ = services.AddSingleton(TimeProvider.System);

    return services;
}

static IServiceCollection SetupScopedServices(IServiceCollection services)
{
    _ = services.AddHttpContextAccessor();

    _ = services
        .AddScoped<IAuthorisationService, AuthorisationService>()
        .AddScoped<IPasswordService, PasswordService>()
        .AddScoped<IPasswordHasher<User>, PasswordHasher<User>>()
        .AddScoped<CreateApiAuditEntry>()
        .AddScoped<ITokenFactory, TokenFactory>();

    _ = services
        .AddScoped<IActivityLogic, ActivityLogic>()
        .AddScoped<IHouseholdSetupLogic, HouseholdSetupLogic>()
        .AddScoped<ILightSceneLogic, LightSceneLogic>()
        .AddScoped<ILightSyncLogic, LightSyncLogic>()
        .AddScoped<IRecipeLogic, RecipeLogic>()
        .AddScoped<IShoppingListLogic, ShoppingListLogic>();

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
