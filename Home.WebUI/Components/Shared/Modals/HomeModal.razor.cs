using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Home.WebUI.Components.Shared.Modals;

public partial class HomeModal
{

    #region Fields

    private readonly string m_PanelID = $"home-modal-{Guid.NewGuid():N}";

    /// <summary>
    /// Whether the panel has been focused since it last opened, so a re-render while the modal is
    /// open does not drag the cursor back out of whatever field the user moved to.
    /// </summary>
    private bool m_Focused;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Whether opening the modal puts the cursor in its first field. Turn it off when that field
    /// edits something that already exists — a settings list whose first row is a rename box
    /// would otherwise invite renaming it by accident.
    /// </summary>
    [Parameter] public bool AutoFocus { get; set; } = true;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// The modal's primary action. Setting it turns the body into a form, which is what gives the
    /// modal its Enter key — see the note in the markup.
    /// </summary>
    [Parameter] public EventCallback OnSubmit { get; set; }

    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!this.Visible)
        {
            this.m_Focused = false;
            return;
        }

        if (this.m_Focused)
            return;

        this.m_Focused = true;

        if (!this.AutoFocus)
            return;

        // A dead circuit cannot focus anything, and that is never worth an error on screen.
        try { await this.JS.InvokeVoidAsync("homeModal.focusFirstField", this.m_PanelID); } catch { }
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task CloseAsync()
        => await this.VisibleChanged.InvokeAsync(false);

    private async Task SubmitAsync()
        => await this.OnSubmit.InvokeAsync();

    #endregion Methods

}
