using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{

    #region Methods

    public void Configure(EntityTypeBuilder<RecipeIngredient> entity)
    {
        _ = entity.ToTable(nameof(RecipeIngredient), DomainValues.Schema);

        _ = entity.HasKey(e => new { e.IngredientID, e.RecipeID });

        _ = entity.Property(e => e.RecipeID);
        _ = entity.HasOne(e => e.Recipe)
            .WithMany(e => e.Ingredients)
            .HasConstraintName("FK_RecipeIngredient_Recipe")
            .HasForeignKey(e => e.RecipeID)
            .OnDelete(DeleteBehavior.Cascade);

        _ = entity.Property(e => e.IngredientID);
        _ = entity.HasOne(e => e.Ingredient)
            .WithMany(e => e.Recipes)
            .HasConstraintName("FK_RecipeIngredient_Ingredient")
            .HasForeignKey(e => e.IngredientID)
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
