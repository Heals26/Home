namespace Home.Domain.Entities;

public class ClientApplication
{

    #region Properties

    public long ClientApplicationID { get; set; }

    public string AccessToken { get; set; }
    public string Name { get; set; }
    public string Secret { get; set; }

    #endregion Properties

}
