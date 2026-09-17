using Home.Domain.Deletions;

namespace Home.Domain.Entities;

public class LightGroup : ISoftDeletable
{

    #region Properties

    public long LightGroupID { get; set; }

    public DateTime? DeletedOnUTC { get; set; }

    /// <summary>
    /// The provider's group ID where this group was seeded from one. Null for groups created in
    /// Home, which the provider knows nothing about. Those are addressed by listing their
    /// lights instead.
    /// </summary>
    public string? ID { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display order on the Lights page. Rooms are not alphabetical in real life.
    /// </summary>
    public int Sequence { get; set; }

    public ICollection<Light> Lights { get; set; } = [];
    public LightLocation Location { get; set; } = null!;

    #endregion Properties

}
