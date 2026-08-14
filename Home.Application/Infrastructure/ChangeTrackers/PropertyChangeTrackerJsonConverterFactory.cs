using System.Text.Json;
using System.Text.Json.Serialization;

namespace Home.Application.Infrastructure.ChangeTrackers;

public class PropertyChangeTrackerJsonConverterFactory : JsonConverterFactory
{

    #region Methods

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(PropertyChangeTracker<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(
            typeof(PropertyChangeTrackerJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;

    #endregion Methods

}
