using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{

    #region Methods

    public void Configure(EntityTypeBuilder<RecipeStep> entity)
    {
        _ = entity.ToTable(nameof(RecipeStep), DomainValues.Schema);

        _ = entity.HasKey(e => e.RecipeStepID);
        _ = entity.Property(e => e.RecipeStepID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Content)
            .IsRequired();

        _ = entity.Property(e => e.Title)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property(e => e.Sequence)
            .IsRequired();

        // Never configured, so EF inferred an optional no-action link from Recipe.Steps and
        // deleting any recipe that had steps failed on the foreign key. The recipe is the only
        // owner, and the household is already reached through it, so this is a single cascade
        // path — the same shape as RecipeIngredient. RecipeStep carries no back-navigation, so
        // the key lives in shadow state.
        _ = entity.Property<long>("RecipeID");
        _ = entity.HasOne<Recipe>()
            .WithMany(e => e.Steps)
            .HasConstraintName("FK_RecipeStep_Recipe")
            .HasForeignKey("RecipeID")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
