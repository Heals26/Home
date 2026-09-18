using Home.WebUI.Infrastructure.CancellationTokens;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Feedback;

public partial class HomeUndoBar
{

    #region Fields

    private readonly CancellationTokenHandler m_CancellationTokenHandler = new();
    private IDisposable? m_Docking;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Where a docked bar sits in whatever holds it. Above it, unless this says otherwise.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Shown inside something that covers the foot of the page, such as an open dialog or the trolley
    /// bar, which holds back the bar at the foot of the page while it is there.
    /// </summary>
    [Parameter] public bool Docked { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        this.UndoLogic.Changed += this.OnUndoChanged;

        if (this.Docked)
            this.m_Docking = this.UndoLogic.Dock();
    }

    public void Dispose()
    {
        this.UndoLogic.Changed -= this.OnUndoChanged;
        this.m_Docking?.Dispose();
        this.m_CancellationTokenHandler.Dispose();
    }

    #endregion Lifecycle Methods

    #region Methods

    private string Classes()
        => this.Docked
            ? this.Class ?? "absolute inset-x-0 bottom-full mb-3"
            : "fixed inset-x-4 bottom-[calc(4.75rem_+_env(safe-area-inset-bottom))] z-40 rail:bottom-6 rail:left-24 rail:right-0 rail:mx-auto rail:max-w-md";

    private void OnUndoChanged()
        => _ = this.InvokeAsync(this.StateHasChanged);

    private bool Shows()
        => this.Docked || !this.UndoLogic.IsDocked;

    private Task UndoAsync()
        => this.UndoLogic.UndoAsync(this.m_CancellationTokenHandler.Token);

    #endregion Methods

}
