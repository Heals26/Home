using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Home.WebUI.Components.Shared.Modals;

public partial class HomeModal : IDisposable
{

    #region Fields

    private readonly string m_PanelID = $"home-modal-{Guid.NewGuid():N}";

    /// <summary>
    /// Whether showModal has been called since the modal last opened, so a re-render while it is
    /// open does not reopen it or drag the cursor back out of whatever field the user moved to.
    /// </summary>
    private bool m_Opened;

    private DotNetObjectReference<HomeModal>? m_Reference;

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
            this.m_Opened = false;
            return;
        }

        if (this.m_Opened)
            return;

        this.m_Opened = true;
        this.m_Reference ??= DotNetObjectReference.Create(this);

        // A dead circuit cannot open anything, and that is never worth an error on screen.
        try { await this.JS.InvokeVoidAsync("homeModal.open", this.m_PanelID, this.AutoFocus, this.m_Reference); } catch { }
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task CloseAsync()
        => await this.VisibleChanged.InvokeAsync(false);

    /// <summary>
    /// Escape, routed back through Blazor rather than left to close the element on its own, so the
    /// DOM and <see cref="Visible"/> never disagree about whether the modal is open.
    /// </summary>
    [JSInvokable]
    public async Task CloseFromBrowserAsync()
    {
        await this.CloseAsync();

        this.StateHasChanged();
    }

    public void Dispose()
    {
        this.m_Reference?.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task SubmitAsync()
        => await this.OnSubmit.InvokeAsync();

    #endregion Methods

}
