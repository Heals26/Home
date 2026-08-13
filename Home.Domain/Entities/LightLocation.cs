namespace Home.Domain.Entities;

public class LightLocation
{

    #region Properties

    public long LightLocationID { get; set; }
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<LightGroup> Groups { get; set; } = [];
    public Household Household { get; set; } = null!;

    #endregion Properties

}
