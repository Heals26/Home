/*
    API audit trail — handy queries.

    Every request through Home.WebApi (bar heartbeats, the SignalR hub and static/Swagger content)
    lands one row in home.ApiAuditEntry. All timestamps are UTC.

    RequestBody is empty by design for /api/OAuth, /api/Households/register and /api/Users — those
    bodies carry passwords and client secrets and are never recorded. Bodies are capped at the
    first 4000 characters.

    Details carries the exception type and message when the request faulted.
*/

-- The last 100 requests.
SELECT TOP (100)
    a.RequestReceivedOnUTC,
    a.HttpResponseStatusCode,
    a.ActionName,
    a.RequestUri,
    a.UserID,
    a.RemoteIPAddress,
    a.CreatedResourceID,
    a.RequestBody
FROM home.ApiAuditEntry AS a
ORDER BY a.ApiAuditEntryID DESC;

-- The last 100 failures (anything that was not a 2xx).
SELECT TOP (100)
    a.RequestReceivedOnUTC,
    a.HttpResponseStatusCode,
    a.ActionName,
    a.RequestUri,
    a.UserID,
    a.Details,
    a.RequestBody
FROM home.ApiAuditEntry AS a
WHERE a.HttpResponseStatusCode NOT BETWEEN 200 AND 299
ORDER BY a.ApiAuditEntryID DESC;

-- Everything hitting one route. Change the LIKE to taste, e.g. '%/api/ShoppingListItems%'.
SELECT
    a.RequestReceivedOnUTC,
    a.HttpResponseStatusCode,
    a.ActionName,
    a.RequestUri,
    a.RequestBody,
    a.CreatedResourceID
FROM home.ApiAuditEntry AS a
WHERE a.RequestUri LIKE '%/api/ShoppingListItems%'
ORDER BY a.ApiAuditEntryID DESC;

-- Everything in a window, in the order it arrived. Useful for reconstructing what the tablet did
-- around the time something went wrong. Times are UTC.
DECLARE @FromUTC DATETIME2 = DATEADD(HOUR, -1, SYSUTCDATETIME());
DECLARE @ToUTC   DATETIME2 = SYSUTCDATETIME();

SELECT
    a.RequestReceivedOnUTC,
    DATEDIFF(MILLISECOND, a.RequestReceivedOnUTC, a.ResponseSentOnUTC) AS DurationMS,
    a.HttpResponseStatusCode,
    a.ActionName,
    a.RequestUri,
    a.UserID,
    a.RequestBody
FROM home.ApiAuditEntry AS a
WHERE a.RequestReceivedOnUTC >= @FromUTC
    AND a.RequestReceivedOnUTC < @ToUTC
ORDER BY a.RequestReceivedOnUTC ASC;

-- Which list did an item actually get posted to? Answers the "it went on the wrong list" report
-- by showing the ShoppingListID the browser sent, per request.
SELECT
    a.RequestReceivedOnUTC,
    a.HttpResponseStatusCode,
    a.CreatedResourceID AS ShoppingListItemID,
    JSON_VALUE(a.RequestBody, '$.shoppingListID') AS PostedShoppingListID,
    JSON_VALUE(a.RequestBody, '$.name')           AS PostedName,
    a.RequestBody
FROM home.ApiAuditEntry AS a
WHERE a.ActionName = 'ShoppingListItems.CreateShoppingListItem'
ORDER BY a.ApiAuditEntryID DESC;
