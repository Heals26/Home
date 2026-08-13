using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.Infrastructure.CancellationTokens;

namespace Home.WebUI.Components.Pages.Authorisation;

public partial class Login
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private bool m_IsLoading;
    private CreatePasswordGrantWebAppRequest m_Model = new();

    #endregion Fields

    #region Methods

    private async Task HandleSubmitAsync()
    {
        this.m_IsLoading = true;

        try
        {
            var _Response = await this.ApiAccess.TryLoginAsync(
                this.m_Model,
                (e) => this.m_ErrorHandler!.AddError(e),
                this.m_CancellationTokenHandler.Token);

            if (_Response)
                this.NavigationManager.NavigateTo("/");
            else
                this.m_ErrorHandler!.AddError("There was an error saving your credentials. Please try again later.");
        }
        catch (Exception _Exception)
        {
            this.m_ErrorHandler!.AddError($"Error: {_Exception.Message}");
            this.StateHasChanged();
        }
        finally
        {
            this.m_IsLoading = false;
        }
    }

    #endregion Methods

}
