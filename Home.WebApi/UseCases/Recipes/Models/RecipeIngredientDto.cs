using Home.Application.Infrastructure.Recipes;

namespace Home.WebApi.UseCases.Recipes.Models;

public class RecipeIngredientDto
{

    #region Properties

    /// <summary>
    /// How much, in <see cref="Unit"/>.
    /// </summary>
    public decimal? Amount { get; set; }

    public long IngredientID { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// What the household knows about buying this ingredient, such as a brand or which shop. It is
    /// held against the ingredient rather than the recipe, so it would read the same in every
    /// recipe sharing that ingredient, except that nothing in the application ever shares one:
    /// adding and importing both create a fresh row, so today a note reaches one recipe.
    /// <para>
    /// The API allows an ingredient several notes. Only the first is carried here, because one line
    /// is what a list of ingredients has room for and what the household actually writes.
    /// </para>
    /// <para>
    /// Empty rather than null when there is no note, so a caller can read it without a guard.
    /// <see cref="NoteID"/> is what says whether one exists.
    /// </para>
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// The ID of the note in <see cref="Note"/>, needed to change or clear it. Null when there is
    /// no note yet.
    /// </summary>
    public long? NoteID { get; set; }

    /// <summary>
    /// Where it sits in this recipe's list — the order it is reached for while cooking.
    /// </summary>
    public long Sequence { get; set; }

    public long? Unit { get; set; }

    /// <summary>
    /// How the unit reads beside the amount, resolved here so every screen says the same thing.
    /// </summary>
    public string UnitAbbreviation
        => MeasurementUnitLogic.GetAbbreviation(this.Unit, this.Amount);

    #endregion Properties

}
