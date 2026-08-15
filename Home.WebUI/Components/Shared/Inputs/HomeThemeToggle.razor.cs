using Microsoft.JSInterop;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeThemeToggle
{

    #region Fields

    private readonly List<HomeSegmentedControl<string>.SegmentOption> m_Options =
    [
        new("Dark", "dark"),
        new("Light", "light"),
        new("Match device", "device")
    ];

    private string m_Preference = "dark";

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var _Stored = await this.JS.InvokeAsync<string>("homeTheme.get");

        if (_Stored == this.m_Preference)
            return;

        this.m_Preference = _Stored;
        this.StateHasChanged();
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task SetPreferenceAsync(string preference)
    {
        this.m_Preference = preference;

        await this.JS.InvokeVoidAsync("homeTheme.set", preference);
    }

    #endregion Methods

}
