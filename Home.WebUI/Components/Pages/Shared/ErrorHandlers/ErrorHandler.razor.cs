using Microsoft.AspNetCore.Mvc;

namespace Home.WebUI.Components.Pages.Shared.ErrorHandlers;

public partial class ErrorHandler
{

    #region Records

    private record ErrorEntry(string Title, string Detail, IReadOnlyList<string> FieldErrors);

    #endregion Records

    #region Fields

    private List<ErrorEntry> m_Errors = new();

    #endregion Fields

    #region Methods

    public void AddError(ValidationProblemDetails errors)
    {
        var _Title = errors.Title ?? string.Empty;
        var _Detail = errors.Detail ?? string.Empty;
        var _FieldErrors = errors.Errors?
            .SelectMany(e => e.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList() ?? [];

        // If there's no title and no detail, fall back to field errors only
        // If there's nothing at all, show a generic message
        if (string.IsNullOrWhiteSpace(_Title) && string.IsNullOrWhiteSpace(_Detail) && !_FieldErrors.Any())
            _Title = "An error occurred.";

        this.m_Errors.Add(new ErrorEntry(_Title, _Detail, _FieldErrors));
        this.StateHasChanged();
    }

    public void AddError(string error)
    {
        this.m_Errors.Add(new ErrorEntry(error, string.Empty, []));
        this.StateHasChanged();
    }

    private void RemoveError(ErrorEntry error)
    {
        this.m_Errors.Remove(error);
        this.StateHasChanged();
    }

    public void ResetErrors()
    {
        this.m_Errors.Clear();
        this.StateHasChanged();
    }

    #endregion Methods

}
