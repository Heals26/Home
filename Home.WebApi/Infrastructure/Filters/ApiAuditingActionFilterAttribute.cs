using Home.Application.UseCases.ApiAuditing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Home.WebApi.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiAuditingActionFilterAttribute : ActionFilterAttribute
{

    #region Methods

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var _ApiAuditEntry = context.HttpContext.RequestServices.GetService<CreateApiAuditEntry>();
        _ApiAuditEntry.ActionData.ActionName = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;

        base.OnActionExecuting(context);
    }

    public override void OnResultExecuted(ResultExecutedContext context)
    {
        if (context.Result is ObjectResult _ObjectResult && _ObjectResult.StatusCode == (int)HttpStatusCode.Created)
        {
            // Implement saving created id
        }
    }

    #endregion Methods

}
