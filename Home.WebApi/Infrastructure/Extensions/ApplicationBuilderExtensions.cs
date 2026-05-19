using Home.WebApi.Middleware;

namespace Home.WebApi.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{

    #region Methods

    public static IApplicationBuilder UseApiAuditing(this IApplicationBuilder builder)
        => builder.UseMiddleware<ApiAuditing>();

    #endregion Methods

}

