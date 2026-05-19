using Home.WebUI.Infrastructure.Enumerations;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Home.WebUI.Infrastructure.ApiProviders.Helpers;

public class RouteType : BaseEnumeration
{

    #region Fields

    public static readonly RouteType Route = new("Route", 1);
    public static readonly RouteType Body = new("Body", 2);
    public static readonly RouteType Form = new("Form", 3);

    #endregion Fields

    #region Constructors

    public RouteType(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public HttpContent? GetHttpRequestMessage<TRequest>(TRequest request)
    {
        if (this == Body)
            return new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        else if (this == Form)
        {
            var _MultipartFormDataContent = new MultipartFormDataContent();
            var _Properties = typeof(TRequest).GetProperties();

            foreach (var _Property in _Properties)
            {
                var _Value = _Property.GetValue(request);
                if (_Value == null)
                    continue;
                if (_Value is Stream _Stream)
                    _MultipartFormDataContent.Add(new StreamContent(_Stream), _Property.Name, _Property.Name);
                else
                {
                    var _JsonPropertyNameAttribute = _Property.GetCustomAttribute<JsonPropertyNameAttribute>();
                    var _FormFieldName = _JsonPropertyNameAttribute?.Name ?? _Property.Name;
                    _MultipartFormDataContent.Add(new StringContent(_Value.ToString()!), _FormFieldName);
                }
            }

            return _MultipartFormDataContent;
        }

        return null;
    }

    public static implicit operator RouteType(string name)
        => FromName<RouteType>(name);

    public static implicit operator RouteType(long value)
        => FromValue<RouteType>(value);

    #endregion Methods

}
