using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomePasswordInput
{

    #region Fields

    private readonly string m_InputID = $"home-input-{Guid.NewGuid():N}";
    private bool m_ShowPassword;

    #endregion Fields

    #region Properties

    /// <summary>
    /// The HTML autocomplete token — "current-password" for sign-in, "new-password" for
    /// anything a password manager should not autofill a saved login into (setup, secrets).
    /// </summary>
    [Parameter] public string? AutoComplete { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Error { get; set; }
    private string ErrorID => $"{this.m_InputID}-error";
    [Parameter] public string? Label { get; set; }
    /// <summary>
    /// The HTML name attribute — browsers use it alongside autocomplete to match saved values.
    /// </summary>
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    #endregion Properties

    #region Methods

    private async Task OnInputChanged(ChangeEventArgs e)
        => await this.ValueChanged.InvokeAsync(e.Value?.ToString() ?? string.Empty);

    private void ToggleVisibility()
        => this.m_ShowPassword = !this.m_ShowPassword;

    private string GetInputClasses()
    {
        var _Base = "w-full bg-ink-800 border rounded-lg pl-4 pr-12 py-3 text-sm text-ink-50 placeholder-ink-500 transition-colors focus:outline-none focus:ring-2 focus:ring-ink-300 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed min-h-[48px]";
        var _Border = string.IsNullOrEmpty(this.Error) ? "border-ink-700" : "border-red-500";
        return $"{_Base} {_Border}";
    }

    #endregion Methods

}
