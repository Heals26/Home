namespace Home.Domain.Entities;

public class Light
{

    #region Properties

    public long LightID { get; set; }
    public string ID { get; set; }
    public string Name { get; set; }

    public LightGroup Group { get; set; }

    #endregion Properties

}
