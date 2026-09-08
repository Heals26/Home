using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Home.WebApi.Presenters.Calendar;

/// <summary>
/// The one answer for trying to change a subscribed event, shared by update and delete so the two
/// screens that can hit it say the same thing.
/// </summary>
public static class CalendarEventReadOnlyProblem
{

    #region Methods

    public static ValidationProblemDetails Create()
        => new()
        {
            Detail = "This event comes from a subscribed calendar. Change it there and Home will pick it up on the next refresh.",
            Status = (int)HttpStatusCode.UnprocessableContent,
            Title = "This event is read-only.",
            Errors = new Dictionary<string, string[]>()
        };

    #endregion Methods

}
