using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Home.WebUI.Components;

public partial class App
{

    #region Fields

    /// <summary>
    /// The pages that write or clear the sign-in cookie. They render statically — a plain HTTP
    /// request with an <see cref="HttpContext"/> — because a cookie can only be set while a
    /// response is being written, and a circuit's response finished long ago. Everything else is
    /// interactive as before.
    /// </summary>
    private static readonly string[] s_StaticallyRenderedPaths = ["/login", "/logout", "/setup"];

    #endregion Fields

    #region Properties

    [CascadingParameter] public HttpContext HttpContext { get; set; } = null!;

    private IComponentRenderMode? PageRenderMode
        => s_StaticallyRenderedPaths.Any(p => this.HttpContext.Request.Path.StartsWithSegments(p))
            ? null
            : new InteractiveServerRenderMode(prerender: false);

    #endregion Properties

}
