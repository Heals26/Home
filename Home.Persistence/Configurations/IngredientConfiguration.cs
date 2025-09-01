using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Ingredient> entity)
    {
        _ = entity.ToTable(nameof(Ingredient), DomainValues.Schema);

        _ = entity.HasKey(e => e.IngredientID);
        _ = entity.Property(e => e.IngredientID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        _ = entity.Property(e => e.Quantity)
            .IsRequired(false);

        _ = entity.Property(e => e.Volumne)
            .IsRequired(false);

        _ = entity.Property(e => e.Weight)
            .IsRequired(false);
    }

    #endregion Methods

}
