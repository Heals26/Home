using Home.WebUI.DataAccess.Lights.Models;
using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Lights;

public partial class LightGroupCard
{

    #region Properties

    [Parameter] public IReadOnlyList<LightGroupDto> AllGroups { get; set; } = [];
    [Parameter] public bool EditMode { get; set; }
    [Parameter, EditorRequired] public LightGroupDto Group { get; set; } = default!;
    [Parameter] public bool IsFirst { get; set; }
    [Parameter] public bool IsLast { get; set; }
    [Parameter] public EventCallback OnDeleted { get; set; }
    [Parameter] public EventCallback OnEffects { get; set; }
    [Parameter] public EventCallback<bool> OnGroupPower { get; set; }
    [Parameter] public EventCallback<(LightDto Light, double Brightness)> OnLightBrightness { get; set; }
    [Parameter] public EventCallback<(LightDto Light, long GroupID)> OnLightMoved { get; set; }
    [Parameter] public EventCallback<(LightDto Light, bool IsOn)> OnLightPower { get; set; }
    [Parameter] public EventCallback<(LightDto Light, ColourPreset Preset)> OnLightPreset { get; set; }
    [Parameter] public EventCallback<int> OnMoved { get; set; }
    [Parameter] public EventCallback<string> OnRenamed { get; set; }

    #endregion Properties

    #region Methods

    private async Task OnNameChanged(ChangeEventArgs e)
    {
        var _Name = e.Value?.ToString()?.Trim();

        if (!string.IsNullOrWhiteSpace(_Name) && _Name != this.Group.Name)
            await this.OnRenamed.InvokeAsync(_Name);
    }

    #endregion Methods

}
