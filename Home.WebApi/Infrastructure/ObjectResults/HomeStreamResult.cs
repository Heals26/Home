using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Infrastructure.ObjectResults;

public class HomeStreamResult : FileStreamResult
{

    #region Constructors

    public HomeStreamResult(Stream fileStream, string contentType) : base(fileStream, contentType) { }

    #endregion Constructors

    #region Properties

    public string FileName { get; set; }

    #endregion Properties

    #region Methods

    public override Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(ActionContext));

        if (!string.IsNullOrWhiteSpace(this.FileName))
            context.HttpContext.Response.Headers.Append("Content-Disposition", $"application; fileName=\"{this.FileName}\"");

        return base.ExecuteResultAsync(context);
    }

    #endregion Methods

}
