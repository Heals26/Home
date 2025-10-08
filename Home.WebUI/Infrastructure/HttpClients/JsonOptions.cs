using System.Text.Json;

namespace Home.WebUI.Infrastructure.HttpClients;

public static class JsonOptions
{

    #region Properties

    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    #endregion Properties

}
