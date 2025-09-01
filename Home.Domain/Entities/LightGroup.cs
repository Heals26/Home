namespace Home.Domain.Entities;

public class LightGroup
{

    #region Properties

    public long LightGroupID { get; set; }
    public string ID { get; set; }
    public string Name { get; set; }

    public ICollection<Light> Lights { get; set; }
    public LightLocation Location { get; set; }

    #endregion Properties

}
