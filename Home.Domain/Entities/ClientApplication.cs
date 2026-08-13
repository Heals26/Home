namespace Home.Domain.Entities;

public class ClientApplication
{

    #region Properties

    public long ClientApplicationID { get; set; }

    public string AccessToken { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;

    #endregion Properties

}
