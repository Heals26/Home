using Home.Application.Infrastructure.ChangeTrackers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Serialization.Metadata;

namespace Home.WebApi.Infrastructure.OpenApi;

/// <summary>
/// A tracker goes over the wire through its own converter, as <c>hasBeenSet</c> and <c>value</c>.
/// The schema generator cannot see through a converter, so without this every tracked property
/// would be described as accepting anything.
/// </summary>
internal class PropertyChangeTrackerSchemaTransformer : IOpenApiSchemaTransformer
{

    #region Methods

    /// <summary>
    /// Trackers are written out at each property rather than shared, because the default name
    /// drops nullability and would give an <c>int</c> tracker and an <c>int?</c> one the same schema.
    /// </summary>
    public static string? CreateSchemaReferenceId(JsonTypeInfo typeInfo)
        => IsPropertyChangeTracker(typeInfo.Type) ? null : OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);

    private static bool IsPropertyChangeTracker(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PropertyChangeTracker<>);

    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var _Type = context.JsonTypeInfo.Type;

        if (!IsPropertyChangeTracker(_Type))
            return;

        schema.Type = JsonSchemaType.Object;
        schema.Properties = new Dictionary<string, IOpenApiSchema>()
        {
            ["hasBeenSet"] = new OpenApiSchema() { Type = JsonSchemaType.Boolean },
            ["value"] = await context.GetOrCreateSchemaAsync(_Type.GetGenericArguments()[0], cancellationToken: cancellationToken)
        };
    }

    #endregion Methods

}
