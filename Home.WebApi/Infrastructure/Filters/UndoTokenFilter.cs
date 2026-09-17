using Home.Application.Services.Security;
using Home.Application.Services.Undo;
using Home.WebApi.Infrastructure.Values;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Home.WebApi.Infrastructure.Filters;

/// <summary>
/// Picks up the token a device sends when it means to offer Undo, before the use case runs, so the
/// request's deletes are held back and its changes kept against that token.
/// </summary>
public class UndoTokenFilter(IUndoScope undoScope, IAuthorisationService authorisationService) : IAsyncActionFilter
{

    #region Methods

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated == true
            && context.HttpContext.Request.Headers.TryGetValue(FrameworkValues.UndoTokenHeader, out var _Header)
            && Guid.TryParse(_Header.ToString(), out var _Token))
        {
            undoScope.HouseholdID = authorisationService.GetHousehold().HouseholdID;
            undoScope.Token = _Token;
        }

        return next();
    }

    #endregion Methods

}
