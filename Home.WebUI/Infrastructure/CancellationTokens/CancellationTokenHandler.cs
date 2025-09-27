namespace Home.WebUI.Infrastructure.CancellationTokens;

public class CancellationTokenHandler : IDisposable
{

    #region Properties

    private CancellationTokenSource m_CancellationTokenSource = new();

    #endregion Properties

    #region Properties

    public CancellationToken Token
        => this.m_CancellationTokenSource.Token;

    #endregion Properties

    #region Methods

    public void Cancel()
        => this.m_CancellationTokenSource.Cancel();

    public void Dispose()
        => this.Cancel();

    #endregion Methods

}
