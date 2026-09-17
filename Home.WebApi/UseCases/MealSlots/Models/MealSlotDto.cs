namespace Home.WebApi.UseCases.MealSlots.Models;

public class MealSlotDto
{

    #region Properties

    public long MealSlotID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public TimeSpan? StartsAt { get; set; }

    #endregion Properties

}
