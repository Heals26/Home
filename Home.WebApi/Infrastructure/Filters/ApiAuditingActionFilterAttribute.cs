using Home.Application.UseCases.ApiAuditing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Home.WebApi.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiAuditingActionFilterAttribute : ActionFilterAttribute
{

    #region Methods

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var _ApiAuditEntry = context.HttpContext.RequestServices.GetService<CreateApiAuditEntry>();

        if (_ApiAuditEntry != null && context.ActionDescriptor is ControllerActionDescriptor _ActionDescriptor)
            _ApiAuditEntry.ActionData.ActionName = $"{_ActionDescriptor.ControllerName}.{_ActionDescriptor.ActionName}";

        base.OnActionExecuting(context);
    }

    public override void OnResultExecuted(ResultExecutedContext context)
    {
        var _ApiAuditEntry = context.HttpContext.RequestServices.GetService<CreateApiAuditEntry>();

        // OutputPortPresenter.CreatedAsync puts the new ID in the Location, so that is where the
        // created resource has to be read back from.
        if (_ApiAuditEntry != null
            && context.Result is CreatedResult _CreatedResult
            && long.TryParse(_CreatedResult.Location, out var _CreatedResourceID))
            _ApiAuditEntry.ActionData.CreatedResourceID = _CreatedResourceID;

        base.OnResultExecuted(context);
    }

    #endregion Methods

}
