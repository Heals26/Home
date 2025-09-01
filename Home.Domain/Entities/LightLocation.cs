namespace Home.Domain.Entities;

public class LightLocation
{

    #region Properties

    public long LightLocationID { get; set; }
    public string ID { get; set; }
    public string Name { get; set; }

    public ICollection<LightGroup> Groups { get; set; }

    #endregion Properties

}
