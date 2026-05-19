using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Home.WebApi.Infrastructure.Filters;

public class ApiVersionHeaderFilter : IOperationFilter
{

    #region Fields

    public const string API_HEADER = "api-version";

    #endregion Fields

    #region Methods

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
        => operation.Parameters.Where(p => p.Name == API_HEADER)
        .ToList()
        .ForEach(p => operation.Parameters.Remove(p));

    #endregion Methods

}
