using Microsoft.JSInterop;

namespace Home.WebUI.Infrastructure.Services.Preferences;

/// <summary>
/// Reads and writes <c>localStorage</c> through <c>preferences.js</c>. A failed interop call, which
/// happens while there is no circuit, answers with an empty string rather than throwing, because a
/// missing preference is the default and never an error.
/// </summary>
public class DevicePreferences(IJSRuntime jsRuntime) : IDevicePreferences
{

    #region Methods

    public async Task<string> GetAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string>("homePreferences.get", cancellationToken, key) ?? string.Empty;
        }
        catch (Exception _Exception) when (_Exception is JSException or InvalidOperationException or TaskCanceledException or JSDisconnectedException)
        {
            return string.Empty;
        }
    }

    public async Task SetAsync(string key, string value, CancellationToken cancellationToken)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("homePreferences.set", cancellationToken, key, value);
        }
        catch (Exception _Exception) when (_Exception is JSException or InvalidOperationException or TaskCanceledException or JSDisconnectedException)
        {
            // The choice just will not survive a reload.
        }
    }

    #endregion Methods

}
