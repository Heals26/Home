using Home.Application.Services.Undo;

namespace Home.Application.Infrastructure.Undo;

public class UndoScope : IUndoScope
{

    #region Properties

    public long? HouseholdID { get; set; }
    public Guid? Token { get; set; }

    #endregion Properties

}
