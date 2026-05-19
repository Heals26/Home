using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Notes.UpdateNote;

public class UpdateNoteApiRequest
{

    #region Properties

    /// <summary>
    /// The updated text content of the note.
    /// </summary>
    public PropertyChangeTracker<string> Content { get; set; }

    #endregion Properties

}
