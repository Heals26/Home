using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Content;

public partial class HomeMarkdown
{

    #region Fields

    /// <summary>
    /// Deliberately minimal. <c>DisableHtml</c> strips the raw-HTML block and inline parsers, so
    /// a tag in the source is literal text by the time it is written out. No extension that can
    /// mint attributes or raw markup is enabled.
    /// </summary>
    private static readonly MarkdownPipeline s_Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    /// <summary>
    /// The complete set of schemes a recipe is allowed to point at. Disabling HTML does not stop
    /// Markdig writing <c>javascript:</c> into an href, so this list is the thing that does.
    /// </summary>
    private static readonly string[] s_SafeSchemes = ["http://", "https://", "mailto:"];

    private MarkupString m_Rendered;
    private string m_RenderedFrom = string.Empty;

    #endregion Fields

    #region Properties

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Content { get; set; }
    /// <summary>
    /// "sm" for a page, "lg" for cooking mode's across-the-kitchen type.
    /// </summary>
    [Parameter] public string Size { get; set; } = "sm";

    #endregion Properties

    #region Lifecycle Methods

    protected override void OnParametersSet()
    {
        var _Content = this.Content ?? string.Empty;

        if (_Content == this.m_RenderedFrom)
            return;

        this.m_RenderedFrom = _Content;
        this.m_Rendered = Render(_Content);
    }

    #endregion Lifecycle Methods

    #region Methods

    private bool HasContent()
        => !string.IsNullOrWhiteSpace(this.Content);

    private static MarkupString Render(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return new MarkupString(string.Empty);

        var _Document = Markdown.Parse(markdown, s_Pipeline);

        // Materialised first: the policy replaces nodes, which would otherwise be a mutation
        // during the walk that produced them.
        foreach (var _Node in _Document.Descendants().ToArray())
        {
            if (_Node is LinkInline _Link)
                ApplyLinkPolicy(_Link);
            else if (_Node is AutolinkInline _Autolink)
                ApplyAutolinkPolicy(_Autolink);
        }

        using var _Writer = new StringWriter();
        var _Renderer = new HtmlRenderer(_Writer);

        s_Pipeline.Setup(_Renderer);
        _ = _Renderer.Render(_Document);
        _Writer.Flush();

        return new MarkupString(_Writer.ToString());
    }

    /// <summary>
    /// A link the household can follow keeps its text and gains the usual new-tab guards; one
    /// pointing anywhere else loses its href entirely and is left as the words it wrapped.
    /// </summary>
    private static void ApplyLinkPolicy(LinkInline link)
    {
        var _Url = SanitiseUrl(link.Url);

        if (_Url == null)
        {
            _ = link.ReplaceBy(new LiteralInline(PlainTextOf(link)), false);
        }
        else
        {
            link.Url = _Url;

            if (!link.IsImage)
                AddLinkGuards(link);
        }
    }

    private static void ApplyAutolinkPolicy(AutolinkInline autolink)
    {
        if (SanitiseUrl(autolink.Url) == null)
            _ = autolink.ReplaceBy(new LiteralInline(autolink.Url), false);
        else
            AddLinkGuards(autolink);
    }

    private static void AddLinkGuards(MarkdownObject link)
    {
        link.GetAttributes().AddProperty("rel", "noopener noreferrer nofollow");
        link.GetAttributes().AddProperty("target", "_blank");
    }

    private static string PlainTextOf(LinkInline link)
        => string.Concat(link.Descendants<LiteralInline>().Select(l => l.Content.ToString()));

    /// <summary>
    /// Returns the address to write, or null when there isn't a safe one. Control and whitespace
    /// characters are removed before the scheme is read because browsers ignore them too — that
    /// is what turns <c>java&amp;#9;script:</c> into a working script URL everywhere else.
    /// The stripped form is what gets written, so the href is exactly what was checked.
    /// Relative and protocol-relative addresses are refused as well: a recipe from someone
    /// else's site has no business pointing at a path on this one.
    /// </summary>
    private static string? SanitiseUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        var _Stripped = new string([.. url.Where(c => !char.IsControl(c) && !char.IsWhiteSpace(c))]);

        return s_SafeSchemes.Any(s => _Stripped.StartsWith(s, StringComparison.OrdinalIgnoreCase))
            ? _Stripped
            : null;
    }

    #endregion Methods

}
