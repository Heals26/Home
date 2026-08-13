namespace Home.Domain.Entities;

public class Note
{

    #region Properties

    public long NoteID { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedOnUTC { get; set; }

    public ICollection<Audit> Audits { get; set; } = [];

    #endregion Properties

}
