using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.Support.CreateFeedback;
using Home.WebUI.DataAccess.Support.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebUI.Components.Pages.Support;

public partial class FeedbackSheet : IDisposable
{

    #region Fields

    private static readonly List<HomeSegmentedControl<FeedbackKind>.SegmentOption> m_Kinds =
    [
        new("Problem", FeedbackKind.Problem),
        new("Idea", FeedbackKind.Idea),
    ];

    private readonly CancellationTokenHandler m_CancellationTokenHandler = new();

    private string m_Content = string.Empty;
    private FeedbackKind m_Kind = FeedbackKind.Problem;

    private ValidationProblemDetails? m_Error;
    private bool m_Sending;
    private bool m_Sent;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Set where the sheet opens from Settings, so the feedback names the page before Settings
    /// rather than Settings itself.
    /// </summary>
    [Parameter] public bool FromSettings { get; set; }

    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    private string ContentLabel
        => this.m_Kind == FeedbackKind.Idea ? "What's your idea?" : "What went wrong?";

    #endregion Properties

    #region Methods

    private async Task SendAsync()
    {
        if (this.m_Sending || string.IsNullOrWhiteSpace(this.m_Content))
            return;

        this.m_Sending = true;
        this.m_Error = null;

        var _Request = await this.FeedbackLogic.BuildRequestAsync(this.m_Kind, this.m_Content, this.FromSettings, this.m_CancellationTokenHandler.Token);

        var _Result = await this.ApiAccess.SendRequestAsync<CreateFeedbackWebAppRequest, CreateFeedbackWebAppResponse>(
            _Request,
            ApiProvider.CreateFeedback(),
            e => this.m_Error = e,
            this.m_CancellationTokenHandler.Token);

        this.m_Sending = false;

        if (_Result != null)
            this.m_Sent = true;
    }

    private async Task OnVisibleChangedAsync(bool visible)
    {
        if (!visible)
            await this.CloseAsync();
    }

    /// <summary>
    /// A draft survives the sheet closing, because a tap on the backdrop should not cost someone
    /// what they wrote. Only feedback that has been sent starts afresh.
    /// </summary>
    private async Task CloseAsync()
    {
        if (this.m_Sent)
        {
            this.m_Content = string.Empty;
            this.m_Kind = FeedbackKind.Problem;
            this.m_Sent = false;
        }

        this.m_Error = null;

        await this.VisibleChanged.InvokeAsync(false);
    }

    public void Dispose()
    {
        this.m_CancellationTokenHandler.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion Methods

}
