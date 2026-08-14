using System.Text.Json;
using System.Text.Json.Serialization;

namespace Home.Application.Infrastructure.ChangeTrackers;

internal class PropertyChangeTrackerJsonConverter<TProperty> : JsonConverter<PropertyChangeTracker<TProperty>>
{

    #region Methods

    public override PropertyChangeTracker<TProperty> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected an object with 'hasBeenSet' and 'value' properties for {typeToConvert.Name}.");

        var _HasBeenSet = false;
        var _Value = default(TProperty);

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var _PropertyName = reader.GetString();
            _ = reader.Read();

            if (string.Equals(_PropertyName, "hasBeenSet", StringComparison.OrdinalIgnoreCase))
                _HasBeenSet = reader.GetBoolean();
            else if (string.Equals(_PropertyName, "value", StringComparison.OrdinalIgnoreCase))
                _Value = JsonSerializer.Deserialize<TProperty>(ref reader, options);
            else
                reader.Skip();
        }

        // Assigning through the Value setter would mark the property as set, so unsent
        // properties must construct as default rather than deserialise property-by-property.
        return _HasBeenSet ? new(_Value!) : default;
    }

    public override void Write(Utf8JsonWriter writer, PropertyChangeTracker<TProperty> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("hasBeenSet", value.HasBeenSet);
        writer.WritePropertyName("value");
        JsonSerializer.Serialize(writer, value.Value, options);
        writer.WriteEndObject();
    }

    #endregion Methods

}
